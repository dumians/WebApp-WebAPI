using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using ToDoList.Common;

namespace ToDoList.Server.Common.Wcf
{
    public class ServiceInspector
    {
        public ICollection<Type> GetFrontendServiceInterfaces(Type type)
        {
            ICollection<Type> result = new HashSet<Type>();

            if (type.IsInterface)
            {
                if (ReflectionUtils.GetAttributes<ServiceContractAttribute>(type).Count > 0)
                {
                    result.Add(type);
                }
            }
            else
            {
                var interfaceTypes = type.GetInterfaces();
                if (interfaceTypes != null)
                {
                    result = result.Concat(interfaceTypes.Where(t => ReflectionUtils.GetAttributes<ServiceContractAttribute>(t).Any())).ToList();
                }
            }

            return result;
        }

        public bool HasFrontendServiceInterface(Type type)
        {
            var interfaces = GetFrontendServiceInterfaces(type);

            return interfaces != null && interfaces.Count > 0;
        }

        public IEnumerable<MethodInfo> GetWcfServiceInterfaceMethodsByAction(Type service, string action)
        {
            // check action against names of OperationContract attributes
            var fesInterfaces = GetFrontendServiceInterfaces(service);
            var operations = fesInterfaces.SelectMany(fes => fes.GetMethods())
                .Where(meth => ReflectionUtils.GetAttributes<OperationContractAttribute>(meth).Any(opAttr => string.Equals(action, opAttr.Name, StringComparison.CurrentCultureIgnoreCase)));
            if (operations.Any())
            {
                return operations;
            }
            else
            {
                // if no named OperationContract found check against names of methods
                var methods = fesInterfaces.SelectMany(fes => fes.GetMethods())
                    .Where(meth => ReflectionUtils.GetAttributes<OperationContractAttribute>(meth).Any(opAttr => string.IsNullOrEmpty(opAttr.Name)))
                    .Where(meth => string.Equals(action, meth.Name, StringComparison.CurrentCultureIgnoreCase));

                return methods;
            }
        }
    }
}
