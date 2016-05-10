// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IServicePermissionChecker.cs" company="">
//   Copyright (c) . All rights reserved.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using ToDoList.Common.Attributes;

namespace ToDoList.Server.Common.PermisionsChecker
{
    public interface IServicePermissionChecker
    {
        void CheckWcfServicePermission(Type service, string action);
        void CheckServicePermission(IEnumerable<ServicePermissionAttribute> permAttribs, string checkedForActionToLog);
    }
}
