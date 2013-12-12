using System.Collections.Generic;
using System.Web.Http;

namespace WebContribContrib.Formatting.Jsonp.SampleWebHost.Controllers{
	public class ValuesController : ApiController {
		// GET api/values
		public IEnumerable<string> Get() {
			return new string[] { "value1", "value2" };
		} 
	}
}