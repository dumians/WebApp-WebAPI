// -----------------------------------------------------------------------
// <copyright file="AuthorizationContextMock.cs" company="">
// 
// </copyright>
// -----------------------------------------------------------------------

using ToDoList.Server.Common.ServerCallContext;

namespace ToDoList.Server.Test
{


    /// <summary>
    /// A mock to be used in tests.
    /// </summary>
    public class AuthorizationContextMock : IAuthorizationContext
    {
        /// <summary>
        /// Does nothing.
        /// </summary>
        public void LoadAuthorizationData()
        {
        }

        /// <summary>
        /// Returns true.
        /// </summary>
        /// <param name="actionIdentifier">the action to test</param>
        /// <returns>true</returns>
        public bool HasPermission(string actionIdentifier)
        {
            return true;
        }


        public void FlushCache()
        {
            throw new System.NotImplementedException();
        }
        
        public bool HasPermission(string actionIdentifier, string equipmentIdentifier)
        {
            return true;
        }


        public System.Collections.Generic.IEnumerable<string> GetIntersectedPermissionIdentifiers(System.Collections.Generic.List<string> actionIdentifiers)
        {
            throw new System.NotImplementedException();
        }


        //System.Collections.Generic.List<EquipmentDto> IAuthorizationContext.GetAuthorizedEquipments(string actionIdentifier)
        //{
        //    throw new System.NotImplementedException();
        //}

        public bool HasAnyPermission()
        {
            throw new System.NotImplementedException();
        }


        public System.Collections.Generic.IEnumerable<string> GetIntersectedPermissionIdentifiers(System.Collections.Generic.IEnumerable<string> actionIdentifiers, long uegrId)
        {
            throw new System.NotImplementedException();
        }


        public bool HasPermissionUnlimited(string actionIdentifier)
        {
            throw new System.NotImplementedException();
        }

        public bool HasAnyPermissionUnlimited(System.Collections.Generic.IEnumerable<string> actionIdentifiers)
        {
            throw new System.NotImplementedException();
        }


        public bool HasPermission(string actionIdentifier, long uegrId)
        {
            throw new System.NotImplementedException();
        }
    }
}
