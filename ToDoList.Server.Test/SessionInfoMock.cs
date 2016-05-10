// -----------------------------------------------------------------------
// <copyright file="SessionInfoMock.cs" company="">
// 
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Security.Principal;
using ToDoList.Common.SessionInfo;

namespace ToDoList.Server.Test
{
    public class SessionInfoMock : SessionInfo
    {
        private string _userName;
        private bool _isAuthenticated;

        public SessionInfoMock()
        {
            Identity = WindowsIdentity.GetCurrent();
            RequestTime = DateTime.Now;
            RequestId = Guid.NewGuid().ToString();
            ActivityId = Guid.NewGuid().ToString();
          
            Active = true;
        }

        public string User
        {
            get
            {
                return _userName;
            }
            set { _userName = value; }
        }

        public bool Authenticated
        {
            get
            {
                return _isAuthenticated;
            }
            set { _isAuthenticated = value; }
        }

        public override bool IsAuthenticated()
        {
            return _isAuthenticated;
        }

        protected override string GetUserId()
        {
            return _userName ?? base.GetUserId();
        }
    }
}
