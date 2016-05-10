using System;
using System.Diagnostics;
using System.Security.Principal;
using ToDoList.Common;
using ToDoList.Common.SessionInfo;

namespace ToDoList.Server.Common.ServerCallContext
{
    /// <summary>
    /// Provides a Child Container Scope
    /// Should be Wrapped in a using() statement to dispose properly
    /// </summary>
    public sealed class ChildContainerScope : IDisposable
    {
        /// <summary>
        /// Logger
        /// </summary>
      //  private static readonly ILog Log = LogManager.GetLogger(LogCategories.Infrastructure);

        /// <summary>
        /// Creats a Child Container Scope 
        /// </summary>
        public ChildContainerScope()
        {
            var aid = System.Diagnostics.Trace.CorrelationManager.ActivityId.ToString();

            // Important Create Child Container before any Data is accessed (and creation of a DbContext is triggered)
            DependencyFactory.CreateChildContainer();

           Debug.WriteLine("Initialize SessionInfo from the local environment");

            try
            {
                var sessionInfo = (SessionInfo)DependencyFactory.Resolve<ISessionInfo>();
                sessionInfo.Identity = WindowsIdentity.GetCurrent();
                sessionInfo.RequestTime = DateTime.Now;
                sessionInfo.RequestId = Guid.NewGuid().ToString();
                sessionInfo.ActivityId = aid;
           
                sessionInfo.Active = true;
                AuthorizationContext.Current.LoadAuthorizationData();
            }
            catch (Exception ex)
            {
               Debug.WriteLine("Error creating ChildContainerScope", ex);
            }
        }

        /// <summary>
        /// Creats a Child Container Scope that uses the given Session Info
        /// </summary>
        /// <param name="sessionInfo">Session Info</param>
        public ChildContainerScope(ISessionInfo sessionInfo)
        {
           Debug.WriteLine("Initialize SessionInfo from the local environment");

            DependencyFactory.CreateChildContainer();

            var oldSessionInfo = (SessionInfo)sessionInfo;
            var newSessionInfo = (SessionInfo)DependencyFactory.Resolve<ISessionInfo>();
            newSessionInfo.Identity = oldSessionInfo.Identity;
            newSessionInfo.RequestTime = oldSessionInfo.RequestTime;
            newSessionInfo.RequestId = oldSessionInfo.RequestId;
            //newSessionInfo.RequestUri 
            newSessionInfo.ActivityId = oldSessionInfo.ActivityId;
            newSessionInfo.System = oldSessionInfo.System;

            newSessionInfo.RequestUri = oldSessionInfo.RequestUri;
   
            newSessionInfo.Active = oldSessionInfo.Active;

            AuthorizationContext.Current.LoadAuthorizationData();
        }

        /// <summary>
        /// Disposes the Child Container Scope (including the created Child Container)s
        /// </summary>
        public void Dispose()
        {
           Debug.WriteLine("Child container can be disposed now");
            DependencyFactory.DisposeChildContainer();
        }
    }
}
