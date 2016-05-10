// -----------------------------------------------------------------------
// <copyright file="SessionInfo.cs" company="">
// 
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Security.Principal;

namespace ToDoList.Common.SessionInfo
{
    /// <summary>
    /// SessionInfo
    /// </summary>
    public class SessionInfo : ISessionInfo
    {
        /// <summary>
        /// Logger
        /// </summary>
       // private static readonly ILog Log = LogManager.GetLogger(LogCategories.Infrastructure);

        /// <summary>
        /// Gets the current instance of SessionInfo
        /// </summary>
        public static ISessionInfo Current
        {
            get
            {
                var instance = DependencyFactory.ResolveSafe<ISessionInfo>();
                if (instance != null)
                    instance = !instance.Active ? null : instance;
                return instance;
            }
        }

        /// <summary>
        /// Request ID
        /// </summary>
        public string RequestId { get; set; }

        /// <summary>
        /// Activity ID (System Diagnostics Trace)
        /// </summary>
        public string ActivityId { get; set; }

        /// <summary>
        /// Request Timestamp
        /// </summary>
        public DateTimeOffset RequestTime { get; set; }

        /// <summary>
        /// Request URI
        /// </summary>
        public string RequestUri { get; set; }

        /// <summary>
        /// Server Intance
        /// </summary>
        public string ServerInstance
        {
            get { return Environment.MachineName; }
        }

        /// <summary>
        /// User ID
        /// </summary>
        public string UserId
        {
            get { return GetUserId(); }
        }

        /// <summary>
        /// Name of the System
        /// </summary>
        public string System { get; set; }


        /// <summary>
        /// Identity
        /// </summary>
        public IIdentity Identity { get; set; }


        public SessionInfo()
        {
        }

     

        /// <summary>
        /// Determines if the user is authenticated
        /// </summary>
        /// <returns></returns>
        public virtual bool IsAuthenticated()
        {
            bool isAuthenticated = false;

            if (Identity != null)
            {
                // ---
                // Just an analysis for task 2367 if some user is not belonging to the Trivadis domain.
                // A hard check should be done inside the UserSecurity service in future.
                // This block has to be removed as the UserSecurity service implements that check.
                var domainAndNameArray = Identity.Name.Split(Convert.ToChar("\\"));
                if (domainAndNameArray.Length > 0)
                {
                    string domain = domainAndNameArray[0];
                    if (string.IsNullOrEmpty(domain) || !"ToDOListAD".Equals(domain.ToLower()))
                    {
                        Debug.WriteLine(string.Format("---!!!--- User '{0}' does not belong to the domain Trivadis! ---!!!---", new Exception("Just to have a stacktrace ;-)"), Identity.Name));
                    }
                }
                // ---

                isAuthenticated = Identity.IsAuthenticated;
            }

            return isAuthenticated;
        }

        public string GetUserId(string anonymousUserId)
        {
            string result = GetUserId();

            if (String.IsNullOrEmpty(result))
            {
                result = anonymousUserId;
            }

            return result;
        }

        protected virtual string GetUserId()
        {
            if (Identity == null)
                return string.Empty;

            var domainAndName = Identity.Name;

            if (string.IsNullOrEmpty(domainAndName))
            {
               Debug.WriteLine("UserId(): identity.Name is null");
                return string.Empty;
            }

            var domainAndNameArray = domainAndName.Split(Convert.ToChar("\\"));

            if (domainAndNameArray.Length == 0)
               Debug.WriteLine("UserId(): domainAndNameArray.Length == 0");
            else if (domainAndNameArray[1] == null)
               Debug.WriteLine("UserId(): domainAndNameArray[1] is null");

            return domainAndNameArray.Length > 1 ? domainAndNameArray[1] : string.Empty;
        }

    

        /// <summary>
        /// Active Flag
        /// </summary>
        public bool Active { get; set; }
    }
}