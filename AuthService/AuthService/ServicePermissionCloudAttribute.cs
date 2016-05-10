using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Web.Http;
using Microsoft.Owin;
using ToDoList.Server.AuthService.Owin;

namespace ToDoList.Server.Cloud.AuthService.AuthService
{
    public class ServicePermissionCloudAttribute : AuthorizeAttribute
    {
        private List<string> m_AllowedRoles = new List<string>();

        //private Guid m_ApplicationID ;
        private IRoleProvider m_RoleProvider;

        private string m_NameClaim;

        private static TraceSource m_TraceSource = new TraceSource("SecManAuthorizationMiddleware");


        public ServicePermissionCloudAttribute()
        {
       
        }

        public ServicePermissionCloudAttribute(string roles) 
        {
            var untrimmerList = roles.Split(',');
            foreach (var role in untrimmerList)
            {
                m_AllowedRoles.Add(role.Trim());
            }

            traceInf("Required roles on action: {0}", string.Join(",", m_AllowedRoles));
        }


        protected override bool IsAuthorized(System.Web.Http.Controllers.HttpActionContext actionContext)
        {   
            var owinEnv = getOwinEnvironment(actionContext);

            if (owinEnv != null)
            {
                if (owinEnv.ContainsKey(SecManAuthorizationMiddleware.OptionsKey))
                {
                    SecManMdwOptions opts = owinEnv[SecManAuthorizationMiddleware.OptionsKey] as SecManMdwOptions;
                    if (opts == null)
                        throw new ArgumentException("SecManAuthorizationMiddleware was not of expected type.");

                    m_NameClaim = opts.NameClaim;
                    m_RoleProvider = opts.RoleProvider;
                }
                else
                {
                    traceInf("Action 'IsAuthorized' method entered, but 'MS_OwinContext' key was not found in environment.");
                    return false;
                }
            
                //var res0 = base.IsAuthorized(actionContext);

                if (actionContext.RequestContext.Principal != null)
                {
                    traceInf("Executing 'IsAuthorized'. Principal found '{0}'", actionContext.RequestContext.Principal.Identity.Name);

                    if (actionContext.RequestContext.Principal.Identity.IsAuthenticated)
                    {
                        // Shows the Windows identity, which the identity of the process, which hosts the service.
                        traceInf("Filter is running under following Windows identity: '{0}'", WindowsIdentity.GetCurrent().Name);

                        ClaimsIdentity identity = actionContext.RequestContext.Principal.Identity as ClaimsIdentity;

                        var secManUserName = identity.Claims.FirstOrDefault(c => c.Type == m_NameClaim);
                        if (secManUserName == null)
                        {
                            traceErr(string.Format("The claim '{0}' cannot be found in the identity. Please ensure that the token contains specified claim.", m_NameClaim));
                            throw new ArgumentException(string.Format("The claim '{0}' cannot be found in the identity. Please ensure that the token contains specified claim.", m_NameClaim));
                        }

                        appendScmRoles(identity, secManUserName.Value);

                        bool res = false;

                        if (m_AllowedRoles.Count > 0)
                        {
                            foreach (var item in m_AllowedRoles)
                            {
                                res = actionContext.RequestContext.Principal.IsInRole(item);
                                if (res) break;
                            }
                        }
                        else
                            res = true; // If no roles explicitelly specified, we authorize all roles.

                        traceInf(res ? "Security Manager user '{0}' authenticated successfully'" : "Security Manager user '{0}' was not authenticated'", secManUserName);

                        return res;
                    }
                    traceInf("Executing 'IsAuthorized'. Identity is not authenticated");
                    return false;
                }
                traceInf("Principal NOT found ");
                return false;
            }
            traceErr("Owin context NOT found!");
            return false;
        }

        private IDictionary<string, object> getOwinEnvironment(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            if (actionContext.Request.Properties.ContainsKey("MS_OwinContext"))
            {
                return ((OwinContext)actionContext.Request.Properties["MS_OwinContext"]).Environment;
            }
            return actionContext.Request.Properties.ContainsKey("MS_OwinEnvironment")
                ? actionContext.Request.Properties["MS_OwinEnvironment"] as IDictionary<string, object>
                : null;
        }


        private Dictionary<string, ICollection<string>> m_Roles = new Dictionary<string, ICollection<string>>();

        private void appendScmRoles(ClaimsIdentity identity, string secManIdentityName)
        {
            if (m_Roles.ContainsKey(secManIdentityName) == false)
            {
                var extractedRoles = m_RoleProvider.GetRoles(identity, secManIdentityName);
                 m_Roles.Add(secManIdentityName, extractedRoles);             
            }

            StringBuilder sb = new StringBuilder();
           
            foreach (var role in m_Roles[secManIdentityName])
            {
                sb.Append(role);
                sb.Append(", ");
                identity.AddClaim(new Claim(ClaimTypes.Role, role));
            }

            traceInf("Provided roles for identity {0} : '{1}'.", secManIdentityName, sb);
        }

        public override bool Match(object obj)
        {
            return base.Match(obj);
        }

        public override void OnAuthorization(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            base.OnAuthorization(actionContext);
        }

        private void traceInf(string txt, params object[] args)
        {
            m_TraceSource.TraceEvent( TraceEventType.Information, 1, String.Format(txt, args));
        }

        private void traceErr(string txt, params object[] args)
        {
            m_TraceSource.TraceEvent(TraceEventType.Error, 1, String.Format(txt, args));
        }
    }
}
