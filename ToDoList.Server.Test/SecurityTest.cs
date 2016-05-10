using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToDoList.Common;
using ToDoList.Common.SessionInfo;
using ToDoList.Server.Common.ServerCallContext;

namespace ToDoList.Server.Test
{
    [TestClass]
    public class SecurityTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            DependencyFactory.RegisterTypeIfMissing<ISessionInfo, SessionInfoMock>();
            DependencyFactory.RegisterTypeIfMissing<IAuthorizationContext, AuthorizationContextMock>();

        }
    }
}
