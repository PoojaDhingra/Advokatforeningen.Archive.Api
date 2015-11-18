using System.Web.Http;

namespace Advokatforeningen.Archive.Api
{
    /// <summary>
    /// web api configuration file
    /// </summary>
    public static class WebApiConfig
    {
        /// <summary>
        /// register http configuration setting
        /// </summary>
        /// <param name="config"></param>
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            config.EnableCors();
            // Web API routes
            config.MapHttpAttributeRoutes();

            //config.Routes.MapHttpRoute(
            //    name: "DefaultApi",
            //    routeTemplate: "api/{controller}/{id}",
            //    defaults: new { id = RouteParameter.Optional }
            //);
        }
    }
}