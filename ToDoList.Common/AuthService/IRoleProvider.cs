using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ToDoList.Server.AuthService.Owin
{
    /// <summary>
    /// Defines a module to be used for authorization. You can implement any module here which
    /// performs authorization of identity. By default, trivadis.SecurityManager.Owin provides a module
    /// which authorize against Security Manager. However you can inject some other module in the SecManAuthorizationMiddleware,
    /// which can be used for authorization. This can also be used to mock Security Manager system if not yet installed.
    /// </summary>
    public interface IRoleProvider
    {
        /// <summary>
        /// Gets roles of identity which will be appended to existing identity roles.
        /// Identity contains already a set of roles provided by authentication server.
        /// This method returns set of roles of idenity from authorization module (server).
        /// </summary>
        /// <param name="identity">Identity as provided by authentication server.</param>
        /// <param name="nameClaim">
        /// The name of identity as known by authorization system.
        /// </param>
        /// <param name="properties">List of proeprties required by authorization module.</param>
        /// <returns></returns>
        ICollection<string> GetRoles(ClaimsIdentity identity, 
            string secManIdentityName);
    }
}
