//-----------------------------------------------------------------------
// <copyright file="AuthorizationContext.cs" company="">
// Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Security;
using System.ServiceModel;
using Microsoft.Practices.Unity;
using ToDoList.Common;
using ToDoList.Common.Cache;
using ToDoList.Common.SessionInfo;
using ToDoList.Server.Common.SecurityService;

namespace ToDoList.Server.Common.ServerCallContext
{
    /// <summary>
    /// Context for User Authorization
    /// </summary>
    public class AuthorizationContext : IExtension<OperationContext>, IAuthorizationContext
    {
        /// <summary>
        /// The KeySeparator
        /// </summary>
        private const string KEY_SEPARATOR = "|";
        
        /// <summary>
        /// The CacheExpiration in Minutes
        /// </summary>
        private const int CACHE_EXPIRATION_MINUTES = 15;

        /// <summary>
        /// The CacheManager
        /// </summary>
        private readonly ICacheManager _cacheManager;

        /// <summary>
        /// The UserSecurityFrontEndService
        /// </summary>
        private readonly IAuthFrontEndService _userSecurityFes;

        /// <summary>
        /// The AuthorizationContextData
        /// </summary>
        private AuthorizationContextData _authorizationContextData;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationContext"/> class. 
        /// </summary>
        /// <param name="cacheManager">the cache to use</param>
        /// <param name="userSecurityFes">the FES to be used to load authorization information</param>
        public AuthorizationContext([Dependency("uthorizationContextData")] ICacheManager cacheManager, IAuthFrontEndService userSecurityFes)
        {
            _cacheManager = cacheManager;
            _userSecurityFes = userSecurityFes;
        }

        /// <summary>
        /// Gets the current Instance from OperationContext or creates a new one
        /// </summary>
        public static IAuthorizationContext Current
        {
            get
            {
                return DependencyFactory.ResolveSafe<IAuthorizationContext>();
            }
        }

        #region Implementation of IExtension<OperationContext>

        /// <summary>
        /// Attach the OperationContext
        /// </summary>
        /// <param name="owner">The OperationContext</param>
        public void Attach(OperationContext owner)
        {
        }

        /// <summary>
        /// Detach the OperationContext
        /// </summary>
        /// <param name="owner">The OperationContext</param>
        public void Detach(OperationContext owner)
        {
        }

        #endregion

        /// <summary>
        /// Loads AuthorizationData from UserSecurityFES to Cache or does nothing if correct data is already in cache
        /// </summary>
        /// <exception cref="ServiceException">if the user is not authenticated</exception>
        public void LoadAuthorizationData()
        {
            var sessionInfo = SessionInfo.Current;

#if (DEBUG)
            using (new ProfilingScope(string.Format("AuthorizationContext.LoadAuthorizationData( \"{0}\" ) @LoadAuthorizationData", sessionInfo.UserId)))
            {
#endif
                if (!sessionInfo.IsAuthenticated())
                {
                    string errMsg = "User \"" + sessionInfo.UserId + "\" is not authenticated!";
                    throw new Exception("UserNotAuthenticated"+ errMsg, new SecurityException(errMsg));
                }

                var cacheKey = GetAuthContextCacheKey(sessionInfo.UserId, sessionInfo.System);

                _authorizationContextData = _cacheManager.AddOrGetExisting(
                    () => InitializeAuthorizationContextData(sessionInfo), TimeSpan.FromMinutes(CACHE_EXPIRATION_MINUTES), cacheKey);
#if (DEBUG)
            }
#endif
        }
        
        /// <summary>
        /// Initializes the AuthorizationContextData.
        /// </summary>
        private AuthorizationContextData InitializeAuthorizationContextData(ISessionInfo sessionInfo)
        {
            return new AuthorizationContextData()
            {
                //TODO : call to Security Service - mock
                SecUserAuthorizationInfo = _userSecurityFes.GetSecAuthorizationInfoList(sessionInfo.UserId, sessionInfo.IsAuthenticated()),
                UserId = sessionInfo.UserId,
               
               
            };
        }

        /// <summary>
        /// GetAuthContextCacheKey for specific attributes
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="systemId"></param>
        /// <param name="plantId"></param>
        /// <returns></returns>
        internal string GetAuthContextCacheKey(string userId, string systemId)
        {
           
            return string.Format("{0}{1}{2}{3}", userId, KEY_SEPARATOR, systemId, KEY_SEPARATOR);
        }

        /// <summary>
        /// Flush the Cache
        /// </summary>
        public void FlushCache()
        {
            _cacheManager.Flush();
        }

        /// <summary>
        /// Has Permission
        /// </summary>
        /// <param name="actionIdentifier">The ActionIdentifier</param>
        /// <returns>true / false</returns>
        public bool HasPermission(string actionIdentifier)
        {
            return _authorizationContextData.HasSecPermission(actionIdentifier);
        }

        /// <summary>
        /// Checks if the current user has any permission.
        /// </summary>
        /// <returns>true if the user has any permission</returns>
        public bool HasAnyPermission()
        {
            return _authorizationContextData.HasAnySecPermission();
        }

        /// <summary>
        /// Has Permission
        /// </summary>
        /// <param name="actionIdentifier">The ActionIdentifier</param>
        /// <param name="equipmentIdentifier">The EquipmentIdentifier (PE or Security Equipment)</param>
        /// <returns>true / false</returns>
        public bool HasPermission(string actionIdentifier, string equipmentIdentifier)
        {
            return _authorizationContextData.HasSecPermission(actionIdentifier);
        }

        /// <summary>
        /// Checks if the current user has the given permission.
        /// </summary>
        /// <param name="actionIdentifier">the action to check</param>
        /// <param name="uegrId">the uegr id to check</param>
        /// <returns>true if the user has the permission to execute the given action</returns>
        public bool HasPermission(string actionIdentifier, long uegrId)
        {
            return _authorizationContextData.HasSecPermission(actionIdentifier);
        }

        /// <summary>
        /// Checks if the current user has the given permission unlimited.
        /// </summary>
        /// <param name="actionIdentifier">the action to check</param>
        /// <returns>true if the user has the permission to execute the given action unlimited</returns>
        public bool HasPermissionUnlimited(string actionIdentifier)
        {
            return _authorizationContextData.HasPermissionUnlimited(actionIdentifier);
        }

        /// <summary>
        /// Checks if the current user has the given permission unlimited.
        /// </summary>
        /// <param name="actionIdentifier">list of the actions to check</param>
        /// <returns>true if the user has the permission to execute the given action unlimited</returns>
        public bool HasAnyPermissionUnlimited(IEnumerable<string> actionIdentifiers)
        {
            return _authorizationContextData.HasAnyPermissionUnlimited(actionIdentifiers);
        }

       

       
        /// <summary>
        /// returns the intersected permissions.
        /// </summary>
        /// <param name="actionIdentifiers">the action identifiers to check if there are intersections</param>
        /// <returns>a list of all permission identifiers which have intersections</returns>
        public IEnumerable<string> GetIntersectedPermissionIdentifiers(List<string> actionIdentifiers)
        {
            return _authorizationContextData.GetIntersectedPermissionIdentifiers(actionIdentifiers);
        }

        /// <summary>
        /// returns the intersected permissions.
        /// </summary>
        /// <param name="actionIdentifiers">the action identifiers to check if there are intersections</param>
        /// <param name="uegrId">the uegr id</param>
        /// <returns>a list of all permission identifiers which have intersections</returns>
        public IEnumerable<string> GetIntersectedPermissionIdentifiers(
            IEnumerable<string> actionIdentifiers,
            long uegrId)
        {
            return _authorizationContextData.GetIntersectedPermissionIdentifiers(actionIdentifiers);
        }

       
    }
}