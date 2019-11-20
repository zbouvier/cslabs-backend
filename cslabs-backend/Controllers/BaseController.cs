using System;
using System.Linq;
using AutoMapper;
using CSLabsBackend.Models;
using CSLabsBackend.Models.UserModels;
using CSLabsBackend.Proxmox;
using FluentEmail.Core;
using Microsoft.AspNetCore.Mvc;

namespace CSLabsBackend.Controllers
{
    public class BaseController: ControllerBase
    {
        protected readonly DefaultContext DatabaseContext;
        private readonly IMapper mapper;
        private User user = null;
        protected ProxmoxApi proxmoxApi;
        private IFluentEmailFactory EmailFactory { get; set; }

        protected IFluentEmail CreateEmail()
        {
            return EmailFactory.Create();
        }

        protected T Map<T>(object value)
        {
            return mapper.Map<T>(value);
        }
        
        public BaseController(BaseControllerDependencies dependencies)
        {
            this.DatabaseContext = dependencies.DatabaseContext;
            this.mapper = dependencies.Mapper;
            this.proxmoxApi = dependencies.ProxmoxApi;
            this.EmailFactory = dependencies.EmailFactory;
        }

        public User GetUser()
        {
            if (user != null)
                return user;
            try
            {
                user = this.DatabaseContext.Users.SingleOrDefault(u => u.Id == int.Parse(this.User.Identity.Name));
                return user;
            }
            catch (ArgumentNullException) { return null; }
            catch (FormatException) { return null; }
            catch (OverflowException) { return null; }
        }
    }
}