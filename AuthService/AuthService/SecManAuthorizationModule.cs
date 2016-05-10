using System;
using System.Collections.Generic;
using System.Linq;
using Daenet.SecurityManager.Client;
using Daenet.SecurityManager.Client.WebService;
using ToDoList.Server.AuthService.Owin;

namespace ToDoList.Server.Cloud.AuthService.AuthService
{
    internal class SecManAuthorizationModule : IRoleProvider
    {
        public Guid ApplicationId { get; set; }

        public ICollection<string> GetRoles(System.Security.Claims.ClaimsIdentity identity, string secManIdentityName)
        {
            SecurityManagerClient secManClient = new SecurityManagerClient();
            var response = secManClient.ResolveIdentity(secManIdentityName, ApplicationId, UserInclude.None);
            return response.Roles.ToArray();
        }
    }
}
