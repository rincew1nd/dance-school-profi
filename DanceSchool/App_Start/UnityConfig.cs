using System.Web.Mvc;
using DanceSchool.Models;
using Unity;
using Unity.Lifetime;
using Unity.Mvc5;

namespace DanceSchool
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
			var container = new UnityContainer();
            
            container.RegisterType<DanceSchoolEntities>(new HierarchicalLifetimeManager());
            
            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
        }
    }
}