//-----------------------------------------------------------------------
// <copyright file="ServicePermissionAttribute.cs" company="">
// Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;

namespace ToDoList.Common.Attributes
{
    /// <summary>
    /// Attribute to define the permission required to access the decorated service.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method /*| AttributeTargets.Class*/, AllowMultiple = true)]
    public class ServicePermissionAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the Name of the required permission.
        /// </summary>
        public string Permission { get; set; }

        /// <summary>
        /// Creates a new attribute holding information about the permission(s) required to access a service.
        /// </summary>
        /// <param name="permission">the required permission</param>
        public ServicePermissionAttribute(string permission)
        {
            this.Permission = permission;
        }
    }
}
