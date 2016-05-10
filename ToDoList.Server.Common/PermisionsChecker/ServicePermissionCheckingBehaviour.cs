// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ServicePermissionCheckingBehaviour.cs" company="">
//   Copyright (c) . All rights reserved.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Practices.Unity.InterceptionExtension;
using ToDoList.Common;
using ToDoList.Common.Attributes;

namespace ToDoList.Server.Common.PermisionsChecker
{
    public class ServicePermissionCheckingBehaviour : IInterceptionBehavior
    {
        //private static readonly ILog Log = LogManager.GetLogger(LogCategories.Infrastructure);

        private IServicePermissionChecker checker = new ServicePermissionChecker();

        /// <summary>
        /// Implement this method to execute your behavior processing.
        /// </summary>
        /// <param name="input">Inputs to the current call to the target.</param>
        /// <param name="getNext">Delegate to execute to get the next delegate in the behavior chain</param>
        /// <returns>Return value from the target.</returns>
        public IMethodReturn Invoke(IMethodInvocation input, GetNextInterceptionBehaviorDelegate getNext)
        {
            CheckServicePermission(input);
            
            return getNext()(input, getNext);
        }

        private void CheckServicePermission(IMethodInvocation input)
        {
            Debug.WriteLine(string.Format("ServicePermissionCheckingBehaviour: checking permissions of {0}.", input.MethodBase.Name));

            checker.CheckServicePermission(ReflectionUtils.GetAttributes<ServicePermissionAttribute>(input.MethodBase), input.MethodBase.Name);
        }

        /// <summary>
        /// Returns the interfaces required by the behavior for the objects it intercepts.
        /// </summary>
        /// <returns>
        /// The required interfaces.
        /// </returns>
        public IEnumerable<Type> GetRequiredInterfaces()
        {
            return Type.EmptyTypes;
        }

        /// <summary>
        /// Optimization hint for proxy generation - will this behavior actually
        /// perform any operations when invoked?
        /// </summary>
        public bool WillExecute
        {
            get { return true; }
        }
    }
}
