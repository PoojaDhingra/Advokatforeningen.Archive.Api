using Microsoft.Practices.Unity;
using System.Web.Http;
using Unity.WebApi;

namespace Advokatforeningen.Archive.Api
{
    /// <summary>
    /// Dependency injection container
    /// </summary>
    public static class UnityConfig
    {
        /// <summary>
        /// register component for unity
        /// </summary>
        public static void RegisterComponents()
        {
            var container = new UnityContainer();

            // register all your components with the container here
            // it is NOT necessary to register your controllers

            // e.g. container.RegisterType<ITestService, TestService>();
            container.RegisterType<Core.IArchiveCase, Repositories.ArchiveCase>();

            GlobalConfiguration.Configuration.DependencyResolver = new UnityDependencyResolver(container);
        }
    }
}