using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using WebApiContrib.Formatting.Jsonp;
using WebContribContrib.Formatting.Jsonp.SampleWebHost.App_Start;

namespace WebContribContrib.Formatting.Jsonp.SampleWebHost {
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class WebApiApplication : System.Web.HttpApplication {
        protected void Application_Start() {
            AreaRegistration.RegisterAllAreas();

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            GlobalConfiguration.Configure(config => {
                config.MapHttpAttributeRoutes();
                FormatterConfig.RegisterFormatters(config.Formatters);
            });
        }
    }
}
