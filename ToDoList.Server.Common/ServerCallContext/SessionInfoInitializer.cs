// -----------------------------------------------------------------------
// <copyright file="SessionInfoInitializer.cs" company="">
// Copyright (c) . All rights reserved.
// </copyright>
// -----------------------------------------------------------------------


using System;
using System.Diagnostics;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Web;
using ToDoList.Common;
using ToDoList.Common.SessionInfo;

namespace ToDoList.Server.Common.ServerCallContext
{
    /// <summary>
    /// SessionInfo Initializer
    /// </summary>
    public static class SessionInfoInitializer
    {

        /// <summary>
        /// Logger
        /// </summary>
        //private static readonly ILog Log = LogManager.GetLogger(LogCategories.Infrastructure);

        /// <summary>
        /// Initializes an instance of SessionInfo from the OperationContext
        /// </summary>
        public static void InitializeFromOperationContext(Message request)
        {
            var headers = new SingleMessageHeaders(() => request.Headers);

            Debug.WriteLine("Initialize SessionInfo from OperationContext");

            DependencyFactory.CreateChildContainer();

            OperationContext.Current.OperationCompleted += DisposeChildContainer;

            var sessionInfo = (SessionInfo)DependencyFactory.Resolve<ISessionInfo>();
            sessionInfo.Identity = GetIdentityFromOperationContext();
            sessionInfo.RequestTime = DateTime.Now;
            sessionInfo.RequestId = headers.RequestId;
            sessionInfo.ActivityId = headers.ActivityId;
          
           

            sessionInfo.RequestUri = request.Headers.To.OriginalString;
          
            sessionInfo.Active = true;

            AuthorizationContext.Current.LoadAuthorizationData();
        }

        /// <summary>
        /// Initializes an instance of SessionInfo from the HttpContext
        /// </summary>
        public static void InitializeFromHttpContext()
        {
            Debug.WriteLine("Initialize SessionInfo from HttpContext");

            DependencyFactory.CreateChildContainer();
            var context = HttpContext.Current;
            
            var sessionInfo = (SessionInfo)DependencyFactory.Resolve<ISessionInfo>();
            if (context.Request.IsAuthenticated)
                sessionInfo.Identity = context.Request.LogonUserIdentity;
            sessionInfo.RequestTime = DateTime.Now;
            sessionInfo.RequestId = Guid.NewGuid().ToString();
            sessionInfo.RequestUri = context.Request.Url.AbsoluteUri;
            sessionInfo.ActivityId = Guid.NewGuid().ToString();
          
          

            //sessionInfo.SetContextUegrId
            sessionInfo.Active = true;

            AuthorizationContext.Current.LoadAuthorizationData();
        }

        public static void DisposeChildContainer(object sender, EventArgs e)
        {
            Debug.WriteLine("Child container can be disposed now");
            DependencyFactory.DisposeChildContainer();
        }

    



        public static IIdentity GetIdentityFromOperationContext()
        {
            IIdentity windowsIdentity = null;

            if (OperationContext.Current == null)
               Debug.WriteLine("GetIdentityFromOperationContext(): OperationContext.Current is null!");

            if (OperationContext.Current != null && OperationContext.Current.ServiceSecurityContext == null)
               Debug.WriteLine("GetIdentityFromOperationContext(): OperationContext.Current.ServiceSecurityContext is null!");

            if (OperationContext.Current != null && OperationContext.Current.ServiceSecurityContext != null)
                windowsIdentity = OperationContext.Current.ServiceSecurityContext.WindowsIdentity;

            return windowsIdentity;
        }
    }
}