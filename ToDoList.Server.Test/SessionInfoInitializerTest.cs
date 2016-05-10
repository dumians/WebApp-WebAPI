using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using ToDoList.Common;
using ToDoList.Common.Cache;
using ToDoList.Common.SessionInfo;
using ToDoList.Server.Common.SecurityService;
using ToDoList.Server.Common.ServerCallContext;
using ToDoList.Server.Common.ServerCallContext.dto;

namespace ToDoList.Server.Test
{
    [TestClass]
    public class SessionInfoInitializerTest
    {
        public TestContext TestContext { get; set; }

        private const string UserName = "RL4USER01";

        private IAuthFrontEndService _stubUserSecurityFes;

        private AuthorizationContextData _authContextData;

        [TestInitialize]
        public void MyTestInitialize()
        {
            var stubCacheManagerAuth = MockRepository.GenerateStub<ICacheManager>();
            var stubCacheManagerEqui = MockRepository.GenerateStub<ICacheManager>();
                _stubUserSecurityFes = MockRepository.GenerateStub<IAuthFrontEndService>();

            DependencyFactory.RenewFactory();

            DependencyFactory.RegisterInstanceIfMissing(() => stubCacheManagerAuth, "AuthorizationContextData");
            DependencyFactory.RegisterInstanceIfMissing(() => stubCacheManagerEqui, "EquipmentModel");
            DependencyFactory.RegisterInstanceIfMissing(() => _stubUserSecurityFes);
           

            _authContextData = new AuthorizationContextData()
            {
                SecUserAuthorizationInfo = CreateSecUserAuthorizationInfoDto(),
                UserId = UserName,
            };

            DependencyFactory.RegisterTypeIfMissingHierachical<IAuthorizationContext, AuthorizationContext>();
              DependencyFactory.RegisterTypeIfMissingHierachical<ISessionInfo, SessionInfo>();
        }

        /// <summary>
        /// Just create a sample UserAuthorizationInfoDto
        /// </summary>
        /// <returns>a sample UserAuthorizationInfoDto</returns>
        private SecUserAuthorizationInfo CreateSecUserAuthorizationInfoDto()
        {
            var secUserAuthorizationInfoDto = new SecUserAuthorizationInfo();
            secUserAuthorizationInfoDto.SetPermissions(new List<SecPermissionDto>());

            return secUserAuthorizationInfoDto;
        }
        
        [TestMethod]
        public void InitializeFromLocalTest()
        {
            _stubUserSecurityFes.Stub(x => x.GetSecAuthorizationInfoList("TESTUSER", true)).Return(_authContextData.SecUserAuthorizationInfo);

            Assert.IsNull(SessionInfo.Current);

            using (new ChildContainerScope())
            {
                var sessionInfo = SessionInfo.Current;
                Assert.IsNotNull(sessionInfo);
                var authorizationContext = AuthorizationContext.Current;
                Assert.IsNotNull(authorizationContext);


                var task1 = new Task(() =>
                {
                    using (new ChildContainerScope())
                    {
                        Assert.AreNotEqual(sessionInfo, SessionInfo.Current);
                        Assert.AreNotEqual(authorizationContext,
                            AuthorizationContext.Current);
                    }
                }, TaskCreationOptions.LongRunning);

           
                var task3 = new Task(() =>
                                         {
                                             using (new ChildContainerScope())
                                             {
                                                 Assert.AreNotEqual(sessionInfo, SessionInfo.Current);
                                                  Assert.AreNotEqual(authorizationContext,
                                                                    AuthorizationContext.Current);
                                               }
                                         }, TaskCreationOptions.LongRunning);

                task1.Start();
                Thread.Sleep(100);
              
                task3.Start();

                task1.Wait();
              
                task3.Wait();
            }
        }

        [TestMethod]
        [Description("Check disposal of child container")]
        [Owner("duj")]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ChildContainerDisposal()
        {
            _stubUserSecurityFes.Stub(x => x.GetSecAuthorizationInfoList("TESTUSER", true)).Return(_authContextData.SecUserAuthorizationInfo);

            Assert.IsNull(SessionInfo.Current);
          

            using (new ChildContainerScope())
            {
                var sessionInfo = SessionInfo.Current;
                Assert.IsNotNull(sessionInfo);
                var authorizationContext = AuthorizationContext.Current;
                Assert.IsNotNull(authorizationContext);
            
               
            }

            
        }

    }
}