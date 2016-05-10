using trivadis.SecurityManager.Client;
using trivadis.SecurityManager.Client.WebService;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace ConsoleApplication1.Auth
{
    public class ScmAuthorizeAttribute : AuthorizeAttribute
    {
        private List<string> m_AllowedRoles = new List<string>();
        public ScmAuthorizeAttribute()
        {
           
        }

        public ScmAuthorizeAttribute(string roles) 
        {
            m_AllowedRoles = roles.Split(',').ToList();
        }

        private Guid m_ApplicationID = new Guid("8c76038a-d382-4efd-8adb-0c0c1b5c6158");

        protected override bool IsAuthorized(System.Web.Http.Controllers.HttpActionContext actionContext)
        {           
            var res0 = base.IsAuthorized(actionContext);

            if (actionContext.RequestContext.Principal != null)
            {
                if (actionContext.RequestContext.Principal.Identity.IsAuthenticated)
                {
                    // Shows the Windows identity, which the identity of the process, which hosts the service.
                    Debug.WriteLine(WindowsIdentity.GetCurrent().Name);

                    ClaimsIdentity identity = actionContext.RequestContext.Principal.Identity as ClaimsIdentity;

                    appendScmRoles(identity, identity.Name);

                    bool res = false;

                    foreach (var item in m_AllowedRoles)
                    {
                        res = actionContext.RequestContext.Principal.IsInRole(item);
                        if (res) break;
                    }
                  
                    return res;
                }
                else
                    return false;
            }
            else
                return false;
        }


        private Dictionary<string, ICollection<string>> m_Roles = new Dictionary<string, ICollection<string>>();

        private void appendScmRoles(ClaimsIdentity identity, string identityClaim)
        {
            if (m_Roles.ContainsKey(identityClaim) == false)
            {
                SecurityManagerClient secManClient = new SecurityManagerClient();
                //secManClient.ClientCredentials.Windows.AllowNtlm = true;
                var response = secManClient.ResolveIdentity(identityClaim, m_ApplicationID, UserInclude.None);
             
                 m_Roles.Add(identityClaim, response.Roles.ToArray());             
            }

            foreach (var role in m_Roles[identityClaim])
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, role));
            }           
        }

        public override bool Match(object obj)
        {
            return base.Match(obj);
        }

        public override void OnAuthorization(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            base.OnAuthorization(actionContext);
        }
    }
}
