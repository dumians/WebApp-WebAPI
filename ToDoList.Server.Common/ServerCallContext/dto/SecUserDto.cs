//-----------------------------------------------------------------------
// <copyright file="SecUserDto.cs" company="">
// Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Runtime.Serialization;

namespace ToDoList.Server.Common.ServerCallContext.dto
{
    /// <summary>
    /// User Security Data Transfer Object
    /// </summary>
    [Serializable]
    public class SecUserDto 
    {
        /// <summary>
        /// Gets or sets the User Id
        /// </summary>
        [DataMember(Name = "Id", Order = 1)]
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the User Loginname
        /// </summary>
        [DataMember(Name = "Loginname", Order = 2)]
        public string Loginname { get; set; }

        /// <summary>
        /// Gets or sets if the user is activated or not
        /// </summary>
        [DataMember(Name = "IsActive", Order = 3)]
        public bool IsActive { get; set; }
    }
}
