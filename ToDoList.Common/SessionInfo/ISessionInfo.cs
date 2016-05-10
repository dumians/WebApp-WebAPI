// -----------------------------------------------------------------------
// <copyright file="ISessionInfo.cs" company="">
//
// </copyright>
// -----------------------------------------------------------------------

using System;
using ToDoList.Common.Attributes;

namespace ToDoList.Common.SessionInfo
{
    /// <summary>
    /// Interface for SessionInfo
    /// </summary>
    [SkipInterception]
    public interface ISessionInfo
    {
        /// <summary>
        /// Request ID
        /// </summary>
        string RequestId { get; }

        /// <summary>
        /// Activity ID (System Diagnostics Trace)
        /// </summary>
        string ActivityId { get; }

        /// <summary>
        /// Request Timestamp
        /// </summary>
        DateTimeOffset RequestTime { get; }

        /// <summary>
        /// Request URI
        /// </summary>
        string RequestUri { get; }

        /// <summary>
        /// Server Intance
        /// </summary>
        string ServerInstance { get; }

        /// <summary>
        /// User ID
        /// </summary>
        string UserId { get; }

        /// <summary>
        /// Name of the System
        /// </summary>
        string System { get; }

    
      
        bool IsAuthenticated();

        /// <summary>
        /// Active Flag
        /// </summary>
        bool Active { get; }

        /// <summary>
        /// Returns the name of the current user or the given name if no current user is logged in.
        /// </summary>
        /// <param name="anonymousUserId">name to return if no user name could be found</param>
        /// <returns>name of the current user or the given name if no current user is logged in</returns>
        string GetUserId(string anonymousUserId);
    }
}