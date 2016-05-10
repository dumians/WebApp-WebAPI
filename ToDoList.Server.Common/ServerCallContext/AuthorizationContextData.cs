using System;
using System.Collections.Generic;
using System.Linq;
using ToDoList.Server.Common.ServerCallContext.dto;

namespace ToDoList.Server.Common.ServerCallContext
{
    /// <summary>
    /// Context Data for User Authorization
    /// </summary>
    [Serializable]
    public class AuthorizationContextData
    {
        public SecUserAuthorizationInfo SecUserAuthorizationInfo { get; set; }

        public string UserId { get; set; }
     
  
        public AuthorizationContextData()
        {
           
        }
        
        /// <summary>
        /// Has User Permission (New Security Linq)
        /// </summary>
        /// <param name="actionIdentifier">the identifier of the action</param>
        /// <returns>true is has permission false if not</returns>
        public bool HasSecPermission(string actionIdentifier)
        {
            var grantedActionList = SecUserAuthorizationInfo.Permissions.Select(x => x.Identifier).ToList();

            return grantedActionList.Any() && grantedActionList.Contains(actionIdentifier, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Checks if the current user has the given permission unlimited.
        /// </summary>
        /// <param name="actionIdentifier">the action to check</param>
        /// <returns>true if the user has the permission to execute the given action unlimited</returns>
        public bool HasPermissionUnlimited(string actionIdentifier)
        {
            var permission = SecUserAuthorizationInfo.Permissions.FirstOrDefault(x => !string.IsNullOrEmpty(x.Identifier) && x.Identifier.Equals(actionIdentifier));

            return permission != null ;
        }

        /// <summary>
        /// Checks if the current user has the given permission unlimited.
        /// </summary>
        /// <param name="actionIdentifier">list of the actions to check</param>
        /// <returns>true if the user has the permission to execute the given action unlimited</returns>
        public bool HasAnyPermissionUnlimited(IEnumerable<string> actionIdentifiers)
        {
            var grantedActionList = SecUserAuthorizationInfo.Permissions.Where(x => !string.IsNullOrEmpty(x.Identifier)).ToList();

            return
                grantedActionList.Any(x => actionIdentifiers.Contains(x.Identifier.ToUpper()));
        }

        /// <summary>
        /// Checks if the current user has any permission.
        /// </summary>
        /// <returns>true if the user has any permission</returns>
        public bool HasAnySecPermission()
        {
            return SecUserAuthorizationInfo.Permissions.Any();
        }
        
   

       
          /// <summary>
        /// returns the intersected permissions.
        /// </summary>
        /// <param name="actionIdentifiers">the action identifiers to check if there are intersections</param>
        /// <returns>a list of all permission identifiers which have intersections</returns>
        public IEnumerable<string> GetIntersectedPermissionIdentifiers(List<string> actionIdentifiers)
        {
            var grantedActionList = SecUserAuthorizationInfo.Permissions;

            if (grantedActionList == null || actionIdentifiers == null)
                return null;

            return
                grantedActionList.Where(x => !string.IsNullOrEmpty(x.Identifier))
                    .Select(x => x.Identifier)
                    .Intersect(actionIdentifiers);
        }

        /// <summary>
        /// returns the intersected permissions.
        /// </summary>
        /// <param name="actionIdentifiers">the action identifiers to check if there are intersections</param>
        /// <param name="uegrId">the uegr id</param>
        /// <returns>a list of all permission identifiers which have intersections</returns>
        public IEnumerable<string> GetIntersectedPermissionIdentifiers(IEnumerable<string> actionIdentifiers)
        {
            var grantedActionList = SecUserAuthorizationInfo.Permissions;

            if (grantedActionList == null || actionIdentifiers == null)
                return null;

            return
                grantedActionList.Where(x => !string.IsNullOrEmpty(x.Identifier) )
                    .Select(x => x.Identifier)
                    .Intersect(actionIdentifiers);
        }
    }
}