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

		// POST api/values
		public void Post(string value) {
		}

		// PUT api/values/5
		public void Put(int id, string value) {
		}

		// DELETE api/values/5
		public void Delete(int id) {
		}
	}
}