using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDoList.Common.Cache
{
    public static class CacheIdentifiers
    {
        public const string TodolistBussineServiceCache  = "ToDoList.BussinesService.Cache";

        public static string[] GetAllCacheNames()
        {
            return new[]
            {
                TodolistBussineServiceCache
            };
        }
    }
}
