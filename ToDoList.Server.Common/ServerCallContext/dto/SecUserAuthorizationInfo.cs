//-----------------------------------------------------------------------
// <copyright file="SecUserAuthorizationInfoDto.cs" company="">
// Copyright (c) . All rights reserved.
// </copyright>

//-----------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace ToDoList.Server.Common.ServerCallContext.dto
{
    /// <summary>
    /// Include all neccessary authorization infors of an User
    /// </summary>
    
    [Serializable]
    public class SecUserAuthorizationInfo
    {
        /// <summary>
        /// Gets or sets user
        /// </summary>
        [DataMember(Name = "User", Order = 1)]
        public SecUserDto User { get; set; }

       
        /// <summary>
        /// Gets or sets permissions
        /// </summary>
        [DataMember(Name = "Permissions", Order = 3)]
        public List<SecPermissionDto> Permissions { get; private set; }

        

        /// <summary>
        /// Set permission list to property
        /// </summary>
        /// <param name="permissions">assigned permissions</param>
        public void SetPermissions(IEnumerable<SecPermissionDto> permissions)
        {
            Permissions = permissions != null ? permissions.ToList() : null;
        }
    }
}
