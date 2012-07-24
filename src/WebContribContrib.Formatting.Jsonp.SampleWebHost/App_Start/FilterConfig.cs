using System.Web;
using System.Web.Mvc;

namespace WebContribContrib.Formatting.Jsonp.SampleWebHost {
	public class FilterConfig {
		public static void RegisterGlobalFilters(GlobalFilterCollection filters) {
			filters.Add(new HandleErrorAttribute());
		}
	}
}