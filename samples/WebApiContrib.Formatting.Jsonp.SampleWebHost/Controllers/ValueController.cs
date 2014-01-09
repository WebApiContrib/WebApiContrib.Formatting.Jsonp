using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace WebContribContrib.Formatting.Jsonp.SampleWebHost.Controllers {
	[Route("api/value/{id:int}/{format?}")]
	public class ValueController : ApiController {

		// GET api/values/5
		public string Get(int id) {
			return "value 1";
		}

		// PUT api/values/5
		public void Put(int id, [FromBody] string value) {
		}

		// DELETE api/values/5
		public void Delete(int id) {
		}
	}
}