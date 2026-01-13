using project_cripta.Models;
using project_cripta.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services.Description;
using Unity;

namespace project_cripta.App_Start
{
    public class UnityConfig
    {
        public static void RegisterComponents()
        {
            var container = new UnityContainer();

            container.RegisterType<IBalanceService, BalanceService>();
            container.RegisterType<ICryptoService, CryptoService>();
            container.RegisterType<IUserService, UserService>();
            container.RegisterType<IRedisService, RedisService>();

            System.Web.Mvc.DependencyResolver.SetResolver(new Unity.Mvc5.UnityDependencyResolver(container));
        }
    }
}