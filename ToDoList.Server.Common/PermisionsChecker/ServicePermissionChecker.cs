// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ServicePermissionChecker.cs" company="">
//   Copyright (c) . All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using ToDoList.Common;
using ToDoList.Common.Attributes;
using ToDoList.Common.SessionInfo;
using ToDoList.Server.Common.ServerCallContext;
using ToDoList.Server.Common.Wcf;

namespace ToDoList.Server.Common.PermisionsChecker
{
    public class ServicePermissionChecker : IServicePermissionChecker
    {
        /// <summary>
        /// The logger used in this class.
        /// </summary>
       // private static readonly ILog Log = LogManager.GetLogger(LogCategories.Infrastructure);

        private readonly ServiceInspector _serviceInspector = new ServiceInspector();

        public ServicePermissionChecker()
        {
        }

        public void CheckWcfServicePermission(Type service, string action)
        {
            CheckServicePermission(GetWcfServicePermissions(service, action), action);
        }

        public void CheckServicePermission(IEnumerable<ServicePermissionAttribute> permAttribs, string checkedForActionToLog)
        {
            CheckServicePermission(permAttribs.Select(permAttrib => permAttrib.Permission), checkedForActionToLog);
        }

        public void CheckServicePermission(IEnumerable<string> permNames, string checkedForActionToLog)
        {
#if (DEBUG)
            using (new ProfilingScope(string.Format("ServicePermissionChecker.CheckServicePermission( \"{0}\" ) @CheckServicePermission", checkedForActionToLog)))
            {
#endif
                if (TrustedCall.IsTrusted()) return;

                var permNameArr = permNames as string[] ;
                if (!permNameArr.Any()) return;

                var userName = SessionInfo.Current.GetUserId("anonymous");

                foreach (var permName in permNameArr)
                {
                   Debug.WriteLine("Checking permission for {0} -> {1}.", userName, permName);

                    // .. check permissions
                    if (AuthorizationContext.Current.HasPermission(permName))
                    {
                        // at least one permission is necessary!
                        return;
                    }
                }

                // ... none of the requires permissions where granted
                var permissions = permNameArr.Any()
                    ? permNameArr.Aggregate((x, y) => x + ", " + y)
                    : "no permissions defined";

                throw new Exception(string.Format("user has no permision {0} {1} {2} ", userName, checkedForActionToLog, permissions ));
#if (DEBUG)
            }
#endif
        }

        public IEnumerable<ServicePermissionAttribute> GetWcfServicePermissions(Type service, string action)
        {
            return GetServiceMethodPermissions(_serviceInspector.GetWcfServiceInterfaceMethodsByAction(service, action));
        }

        public bool HasServicePermission(Type serviceClassOrInterface)
        {
            return serviceClassOrInterface.GetMethods().Any(meth => ReflectionUtils.GetAttributes<ServicePermissionAttribute>(meth, true).Any());
        }

        public IEnumerable<ServicePermissionAttribute> GetServiceMethodPermissions(IEnumerable<MethodInfo> methodInfos)
        {
            var accumulatedPermissions = new List<ServicePermissionAttribute>();
            var methodInfoList = methodInfos as IList<MethodInfo> ?? methodInfos.ToList();

            if (!methodInfoList.Any()) return accumulatedPermissions;

            var firstMethod = true;
            foreach (var methodInfo in methodInfoList)
            {
                var methodToCheck = methodInfo;

                if (methodInfo.DeclaringType.IsClass)
                {
                    // if the method was taken from a class get the according method of the interface
                    methodToCheck = GetInterfaceMethod(methodInfo);

                    if (methodToCheck == null)
                    {
                        throw new Exception("Could not get method called '" + methodInfo + "' defined on '" + methodInfo.DeclaringType + "' from interface!");
                    }
                }

                var newMethodPermissions = ReflectionUtils.GetAttributes<ServicePermissionAttribute>(methodToCheck, true);
                var newMethodPermissionsCount = newMethodPermissions.Count();

                if (firstMethod)
                {
                    accumulatedPermissions = newMethodPermissions.ToList();
                    firstMethod = false;
                }
                else
                {
                    var intersectCount = accumulatedPermissions.Intersect(newMethodPermissions).Count();
                    if (intersectCount == newMethodPermissionsCount && intersectCount == accumulatedPermissions.Count())
                        continue;

                    var exp = string.Format("{0} {1}", methodToCheck.Name, methodToCheck.ReflectedType.FullName);
                    Debug.WriteLine(exp);

                    throw new Exception(exp);
                }
            }

            return accumulatedPermissions;
        }

        private MethodInfo GetInterfaceMethod(MethodInfo classMethodInfo)
        {
            var interfaceMethods = classMethodInfo.DeclaringType.GetInterfaces()
                .SelectMany(interf => interf.GetMethods().Where(meth => string.Equals(meth.Name, classMethodInfo.Name, StringComparison.CurrentCulture)))
                .Where(meth => meth.GetParameters().SequenceEqual(classMethodInfo.GetParameters()));
            return interfaceMethods.FirstOrDefault();
        }
    }
}
