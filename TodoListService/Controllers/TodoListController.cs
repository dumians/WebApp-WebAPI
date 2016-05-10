//----------------------------------------------------------------------------------------------
//    Copyright 2014 Microsoft Corporation
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//----------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

// The following using statements were added for this sample.
using System.Collections.Concurrent;
using TodoListService.Models;
using System.Security.Claims;
using System.Configuration;
using System.Web;
using Microsoft.Practices.Unity;
using ToDoList.Common.Cache;

namespace TodoListService.Controllers
{
    //[Authorize]
    [Authorize(Roles = "TRT_SPOT")]
    public class TodoListController : ApiController
    {
        /// <summary>
        /// Cache Manager - injected
        /// need to be registeed in container on App bootstrap
        /// </summary>
        [Dependency(CacheIdentifiers.TodolistBussineServiceCache)]
        public ICacheManager CacheManager { get; set; }

        //
        // To Do items list for all users.  Since the list is stored in memory, it will go away if the service is cycled.
        //
        static ConcurrentBag<TodoItem> todoBag = new ConcurrentBag<TodoItem>();
        private static string trustedCallerClientId = ConfigurationManager.AppSettings["todo:TrustedCallerClientId"];

        // GET api/todolist
        [Authorize(Roles = "TRT_SPOT.READ")]
        public IEnumerable<TodoItem> Get()
        {
            //
            // If the Owner ID parameter has been set, the caller is trying the trusted sub-system pattern.
            // Verify the caller is trusted, then return the To Do list for the specified Owner ID.
            //
            string ownerid = HttpContext.Current.Request.QueryString["ownerid"];
            if (ownerid != null)
            {
                string currentCallerClientId = ClaimsPrincipal.Current.FindFirst("appid").Value;
                if (currentCallerClientId == trustedCallerClientId)
                {
                    return GetTodoItemsbyOwner(ownerid);
                }
                throw new HttpResponseException(
                    new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.Unauthorized,
                        ReasonPhrase =
                            "Only trusted callers can return any user's To Do List.  Caller's OID:" +
                            currentCallerClientId
                    });
            }

            //
            // The Scope claim tells you what permissions the client application has in the service.
            // In this case we look for a scope value of user_impersonation, or full access to the service as the user.
            //
            Claim scopeClaim = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/scope");
            if (scopeClaim != null && scopeClaim.Value != "user_impersonation")
            {
                throw new HttpResponseException(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Unauthorized,
                    ReasonPhrase = "The Scope claim does not contain 'user_impersonation' or scope claim not found"
                });
            }

            // A user's To Do list is keyed off of the Object Identifier claim, which contains an immutable, unique identifier for the user.
            Claim subject =
                ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier");

            // using Distributed Cache
            //return GetItemsFromCache(subject.Value);


            //get direct without cache
            return GetTodoItemsbyOwner(subject.Value);

        }

        // POST api/todolist
        [Authorize(Roles = "TRT_SPOT.UPDATE")]
        public void Post(TodoItem todo)
        {
            //
            // If the caller is the trusted caller, then add the To Do item to owner's To Do list as specified in the posted item.
            //
            Claim currentCallerClientIdClaim = ClaimsPrincipal.Current.FindFirst("appid");
            if (currentCallerClientIdClaim != null)
            {
                string currentCallerClientId = currentCallerClientIdClaim.Value;
                if (currentCallerClientId == trustedCallerClientId)
                {
                    todoBag.Add(new TodoItem {Title = todo.Title, Owner = todo.Owner});
                    return;
                }
            }

            Claim scopeClaim = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/scope");
            if (scopeClaim != null && scopeClaim.Value != "user_impersonation")
            {
                throw new HttpResponseException(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Unauthorized,
                    ReasonPhrase = "The Scope claim does not contain 'user_impersonation' or scope claim not found"
                });
            }

            if (string.IsNullOrWhiteSpace(todo?.Title))
                return;

            var activeItem = new TodoItem
            {
                Title = todo.Title,
                Owner =
                    ClaimsPrincipal.Current.FindFirst(
                        "http://schemas.microsoft.com/identity/claims/objectidentifier").Value
            };
            todoBag.Add(activeItem);

            //use distributed cache 
            //var cacheKey = string.Format("{0}#ToDoItemsFor#{1}", activeItem.Title, activeItem.Owner);
            //CacheManager.AddWithDefaultSlidingTime(activeItem, cacheKey);
        }

        /// <summary>
        /// Get From DistributedCache
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        protected IEnumerable<TodoItem> GetTodoItemsbyOwner(string Owner)
        {
            if (Owner == null)
                throw new ArgumentNullException("Owner");

            return from todo in todoBag
                where todo.Owner == Owner
                select todo;
        }

        /// <summary>
        ///  Get items from cache
        /// </summary>
        /// <param name="Owner"></param>
        /// <returns></returns>
        protected IEnumerable<TodoItem> GetItemsFromCache(string Owner)
        {
            var cachekey = string.Format("{0}TodoItemFromCache#name#", Owner);
            Func<IEnumerable<TodoItem>> consumersSrc = () => GetTodoItemsbyOwner(cachekey).ToList();

            return CacheManager != null ? CacheManager.AddOrGetExisting(consumersSrc, TimeSpan.FromHours(4), cachekey) : consumersSrc.Invoke();
        }
    }
}
