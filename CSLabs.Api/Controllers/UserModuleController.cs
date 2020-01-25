using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CSLabs.Api.Models.UserModels;
using CSLabs.Api.ResponseModels;
using Microsoft.AspNetCore.Mvc;
using CSLabs.Api.Models;
using CSLabs.Api.Models.ModuleModels;
using CSLabs.Api.Proxmox;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace CSLabs.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserModuleController : BaseController
    {
        public UserModuleController(BaseControllerDependencies deps) : base(deps)
        {
        }

        [HttpPost("{specialCode}")]
        public async Task<IActionResult> Post(string specialCode)
        {
            var module = await DatabaseContext.Modules
                .Include(m => m.Labs)
                .ThenInclude(l => l.LabVms)
                .ThenInclude(v => v.VmTemplates)
                .ThenInclude(t => t.HypervisorNode)
                .ThenInclude(n => n.Hypervisor)
                .FirstAsync(m => m.SpecialCode == specialCode);
            
            if (module == null)
                return BadRequest(new ErrorResponse {Message = "Module not found"});

            var count = await DatabaseContext.UserModules
                .Where(m => m.ModuleId == module.Id)
                .Where(m => m.UserId == GetUser().Id)
                .CountAsync();
            
            if (count > 0)
                return Forbid("Cannot Create Multiple Instances");

            var firstLab = module.Labs.First();
           
            var userModule = new UserModule
            {
                Module = module,
                User = GetUser(),
                UserLabs = new List<UserLab>
                {
                    await firstLab.Instantiate(ProxmoxManager, GetUser())
                }
            };
            DatabaseContext.UserModules.Add(userModule);
            await DatabaseContext.SaveChangesAsync();
            return Ok(userModule);
        }

        [HttpGet("{id}/status")]
        public async Task<IActionResult> Status(int id)
        {
            var userModule = DatabaseContext.UserModules
                .Include(u => u.UserLabs)
                .ThenInclude(l => l.HypervisorNode)
                .ThenInclude(n => n.Hypervisor)
                .Include(u => u.UserLabs)
                .ThenInclude(l => l.UserLabVms)
                .FirstOrDefault(m => m.UserId == GetUser().Id && m.Id == id);
            if (userModule == null)
                return NotFound();
            var userLab = userModule.UserLabs.First();
            var api = ProxmoxManager.GetProxmoxApi(userLab);
            foreach (var vm in userModule.UserLabs.First().UserLabVms)
            {
                var status = await api.GetVmStatus(vm.ProxmoxVmId);
                if (status.Lock == "clone")
                    return Ok("Initializing");
            }

            return Ok("Initialized");
        }


        [HttpGet]
        public IActionResult Get()
        {
            return Ok(DatabaseContext.UserModules
                .Where(m => m.UserId == GetUser().Id)
                .Include(u => u.Module)
                .ToList());
        }

        //Get api
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var module = await DatabaseContext.UserModules
                .Include(u => u.Module)
                .Include(u => u.UserLabs)
                .ThenInclude(l => l.UserLabVms)
                .ThenInclude(vm => vm.LabVm)
                .Include(u => u.UserLabs)
                .ThenInclude(u => u.Lab)
                .FirstOrDefaultAsync(m => m.UserId == GetUser().Id && m.Id == id);
            if (module == null)
                return NotFound();

            if (module.UserId == GetUser().Id)
                return Ok(module);
            return Forbid();
        }
    }
}