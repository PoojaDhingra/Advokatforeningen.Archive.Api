using System.Web.Http;

namespace Advokatforeningen.Archive.Api
{
    /// <summary>
    /// global asax
    /// </summary>
    public class WebApiApplication : System.Web.HttpApplication
    {
        /// <summary>
        /// this method is hit every time when project starts.
        /// </summary>
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            UnityConfig.RegisterComponents();
        }
    }
}