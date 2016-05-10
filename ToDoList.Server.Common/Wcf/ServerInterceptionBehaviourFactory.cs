// -----------------------------------------------------------------------
// <copyright file="ServerInterceptionBehaviourFactory.cs" company="">
// Copyright (c) . All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.InterceptionExtension;
using ToDoList.Common;
using ToDoList.Server.Common.PermisionsChecker;

namespace ToDoList.Server.Common.Wcf
{
    public class ServerInterceptionBehaviourFactory : IInterceptionBehaviourFactory
    {
        //private static readonly ILog Log = LogManager.GetLogger(LogCategories.Infrastructure);

        private readonly ServicePermissionChecker servicePermissionChecker = new ServicePermissionChecker();

        public IEnumerable<InjectionMember> CreateInterceptionBehaviours(Type interfaceType)
        {
            var result = new List<InjectionMember>();

            if (interfaceType.Namespace == null || !interfaceType.Namespace.StartsWith("")) return result;

            if (servicePermissionChecker.HasServicePermission(interfaceType))
            {
                Debug.WriteLine(string.Format("Adding ServicePermissionCheckingBehaviour for service interface {0}.", interfaceType.Name));

                result.Add(new DefaultInterceptionBehavior<ServicePermissionCheckingBehaviour>());
            }
            else
            {
                Debug.WriteLine(string.Format("No ServicePermissionCheckingBehaviour needed for service interface {0}.", interfaceType.Name));
            }

            return result;
        }
    }
}
