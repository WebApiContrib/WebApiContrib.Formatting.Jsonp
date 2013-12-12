using System.Collections.Generic;
using System.Web.Http;

namespace WebContribContrib.Formatting.Jsonp.SampleWebHost.Controllers{
	[Route("api/values/{format}")]
	public class ValuesController : ApiController {
		// GET api/values
		public IEnumerable<string> Get() {
			return new string[] { "value1", "value2" };
		} 
	}
}