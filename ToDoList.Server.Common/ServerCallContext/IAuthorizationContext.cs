using System.Collections.Generic;

namespace ToDoList.Server.Common.ServerCallContext
{
    /// <summary>
    /// Interface for AuthorizationContext
    /// </summary>
    public interface IAuthorizationContext
    {
        /// <summary>
        /// Loads AuthorizationData from UserSecurityFES to Cache or does nothing if correct data is already in cache
        /// </summary>
        /// <exception cref="ServiceException">if the user is not authenticated</exception>
        void LoadAuthorizationData();

        /// <summary>
        /// Flush the Cache
        /// </summary>
        void FlushCache();

        /// <summary>
        /// Checks if the current user has the given permission.
        /// </summary>
        /// <param name="actionIdentifier">the action to check</param>
        /// <returns>true if the user has the permission to execute the given action</returns>
        bool HasPermission(string actionIdentifier);

        /// <summary>
        /// Checks if the current user has the given permission unlimited.
        /// </summary>
        /// <param name="actionIdentifier">the action to check</param>
        /// <returns>true if the user has the permission to execute the given action unlimited</returns>
        bool HasPermissionUnlimited(string actionIdentifier);

        /// <summary>
        /// Checks if the current user has the given permission unlimited.
        /// </summary>
        /// <param name="actionIdentifier">list of the actions to check</param>
        /// <returns>true if the user has the permission to execute the given action unlimited</returns>
        bool HasAnyPermissionUnlimited(IEnumerable<string> actionIdentifiers);

        /// <summary>
        /// Checks if the current user has any permission.
        /// </summary>
        /// <returns>true if the user has any permission</returns>
        bool HasAnyPermission();

        /// <summary>
        /// returns the intersected permissions.
        /// </summary>
        /// <param name="actionIdentifiers">the action identifiers to check if there are intersections</param>
        /// <returns>a list of all permission identifiers which have intersections</returns>
        IEnumerable<string> GetIntersectedPermissionIdentifiers(List<string> actionIdentifiers);

        /// <summary>
        /// returns the intersected permissions.
        /// </summary>
        /// <param name="actionIdentifiers">the action identifiers to check if there are intersections</param>
        /// <param name="uegrId">the uegr id</param>
        /// <returns>a list of all permission identifiers which have intersections</returns>
        IEnumerable<string> GetIntersectedPermissionIdentifiers(IEnumerable<string> actionIdentifiers, long uegrId);

        /// <summary>
        /// Checks if the current user has the given permission.
        /// </summary>
        /// <param name="actionIdentifier">the action to check</param>
        /// <param name="equipmentIdentifier">the equipment (PE or Security Equipment) to check</param>
        /// <returns>true if the user has the permission to execute the given action</returns>
        bool HasPermission(string actionIdentifier, string equipmentIdentifier);

        /// <summary>
        /// Checks if the current user has the given permission.
        /// </summary>
        /// <param name="actionIdentifier">the action to check</param>
        /// <param name="uegrId">the uegr id to check</param>
        /// <returns>true if the user has the permission to execute the given action</returns>
        bool HasPermission(string actionIdentifier, long uegrId);

    

       
    }
}