//-----------------------------------------------------------------------
// <copyright file="IUserSecurityFES.cs" company="">
// Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.ServiceModel;
using ToDoList.Server.Common.ServerCallContext.dto;

namespace ToDoList.Server.Common.SecurityService
{
    /// <summary>
    /// Interface User Security Front End Service
    /// </summary>
    //[XmlComments]
    [ServiceContract]
    public interface IAuthFrontEndService
    {
        

        /// <summary>
        /// Get all permissions of an User
        /// </summary>
        /// <param name="loginName">login name of user</param>
        /// <param name="isAuthenticated">is user authenticated via AD</param>
        /// <returns>User Security Attribute Data Transfer Object</returns>
        [OperationContract]
        SecUserAuthorizationInfo GetSecAuthorizationInfoList(string loginName, bool isAuthenticated);

        }
}
