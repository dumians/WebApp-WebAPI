using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TodoListWcfService;
using ToDoList.Common.Cache;
using  Microsoft.Practices.Unity;

namespace ToDoList.Server.Bussines.Services
{
   

        // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
        // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
        public class TodoListBussinesService : IBussinesService
        {

            /// <summary>
            /// Cache Manager
            /// </summary>
            [Microsoft.Practices.Unity.Dependency(CacheIdentifiers.TodolistBussineServiceCache)]
            public ICacheManager CacheManager { get; set; }

            public string GetData(int value)
            {
                return $"You entered: {value}";
            }

            public CompositeType GetDataUsingDataContract(CompositeType composite)
            {
                if (composite == null)
                {
                    throw new ArgumentNullException(nameof(composite));
                }
                if (composite.BoolValue)
                {
                    composite.StringValue += "Suffix";
                }
                return composite;
            }
        }

}
