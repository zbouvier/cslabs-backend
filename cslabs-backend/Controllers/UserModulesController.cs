using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using CSLabsBackend.Models;
using CSLabsBackend.Models.ModuleModels;
using CSLabsBackend.Models.UserModels;
using CSLabsBackend.Proxmox;
using CSLabsBackend.ResponseModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace CSLabsBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserModulesController : BaseController
    {
        public UserModulesController(DefaultContext defaultContext, IMapper mapper) : base(defaultContext, mapper)
        {
        }
        
        
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] int moduleId)
        {
            var module = DatabaseContext.Modules
                .Include(m => m.Labs)
                .ThenInclude(l => l.LabVms)
                .First(m => m.Id == moduleId);
            if (module == null)
                return BadRequest(new ErrorResponse() {Message = "Module not found"});
            var firstLab = module.Labs.First();
            var api = new ProxmoxApi("http://", "***REMOVED***", "***REMOVED***");
            int createdVmId = await api.CloneTemplate(100);
            var userModule = new UserModule
            {
                Module = module,
                User = GetUser(),
                UserLabs = new List<UserLab>() {new UserLab()
                {
                    Lab = firstLab, 
                    Status = "ON", 
                    User = GetUser(),
                    UserLabVms = new List<UserLabVm>()
                    {
                        new UserLabVm()
                        {
                            LabVm = firstLab.LabVms.First(),
                            User = GetUser(),
                            ProxmoxVmId = createdVmId,
                        }
                    }
                    
                }}
            };
            DatabaseContext.UserModules.Add(userModule);
            await DatabaseContext.SaveChangesAsync();
            return Ok(userModule);
        }
        
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(DatabaseContext.UserModules.Where(m => m.UserId == GetUser().Id).ToList());
        }
        
        //Get api
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var module =  this.DatabaseContext.UserModules.Find(id);
            if (module.UserId == GetUser().Id)
                return Ok(module);
            return Forbid();
        }
    }
}