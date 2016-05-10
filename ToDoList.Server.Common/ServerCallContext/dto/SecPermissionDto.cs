//-----------------------------------------------------------------------
// <copyright file="SecPermissionDto.cs" company="">
// Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Runtime.Serialization;

namespace ToDoList.Server.Common.ServerCallContext.dto
{
    /// <summary>
    /// User Permission Security Data Transfer Object
    /// </summary>
    [Serializable]
    public class SecPermissionDto 
    {
        /// <summary>
        /// Gets or sets the Permission Id
        /// </summary>
        [DataMember(Name = "Id", Order = 1)]
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the Permission identifier
        /// </summary>
        [DataMember(Name = "Identifier", Order = 2)]
        public string Identifier { get; set; }

        /// <summary>
        /// Gets or sets if the Permission is activated or not
        /// </summary>
        [DataMember(Name = "IsActive", Order = 3)]
        public bool IsActive { get; set; }

       

        public override string ToString()
        {
            return string.Format("SecPermissionDto[${0}]", Identifier);
        }
    }
}
