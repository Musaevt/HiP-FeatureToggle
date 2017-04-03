using System.Text;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaderbornUniversity.SILab.Hip.FeatureToggle.Managers;
using System;

namespace PaderbornUniversity.SILab.Hip.FeatureToggle.Controllers
{
	/// <summary>
	/// Sample Values controller -- remove for actual service implementation.
	/// </summary>
    [Route("api/[controller]")]
	[Authorize]
    public class ValuesController : Controller
    {		private readonly ValuesManager _manager;


		public ValuesController(ValuesManager manager): base()
		{
			_manager = manager;
		}

        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
			return _manager.Get();
        }

        // GET api/values/5
        [HttpGet("{id}")]
		public string Get([FromRoute]int id)
        {
			return "";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string val)
        {
			_manager.Add(val);
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete([FromRoute]int id)
        {
        }
    }
}
