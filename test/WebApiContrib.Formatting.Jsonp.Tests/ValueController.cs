using System.Web.Http;

namespace WebContribContrib.Formatting.Jsonp.Tests
{
    public class ValueController : ApiController
    {
        // GET api/values/5
        public string Get(int id)
        {
            return "value 1";
        }
    }
}
