using System.Collections.Generic;
using System.Threading;
using System.Web.Http;
using ToDoList.Server.Cloud.AuthService.AuthService;

namespace ToDoList.Server.Cloud.AuthService.Controllers
{
    [Authorize]
    public class ValuesController : ApiController
    {
        // GET api/values 

        [ServicePermissionCloud("Toolmanagement")]//See RoleProvider.
        public IEnumerable<string> Get()
        {
            return new[] { "value1", "value2", Thread.CurrentPrincipal.Identity.Name, Thread.CurrentPrincipal.IsInRole("Toolmanagement").ToString() };
        }

        // GET api/values/5 
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values 
        public void Post(string value)
        {
        }

        // PUT api/values/5 
        public void Put(int id, string value)
        {
        }

        // DELETE api/values/5 
        public void Delete(int id)
        {
        }
    }
}
