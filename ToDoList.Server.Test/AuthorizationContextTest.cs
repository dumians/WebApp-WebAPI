// -----------------------------------------------------------------------
// <copyright file="Context.cs" company="Trivadis">
// Copyright (c) Trivadis AG. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using Rhino.Mocks.Interfaces;
using ToDoList.Common;
using ToDoList.Common.Cache;
using ToDoList.Common.SessionInfo;
using ToDoList.Server.Common.SecurityService;
using ToDoList.Server.Common.ServerCallContext;
using ToDoList.Server.Common.ServerCallContext.dto;

namespace ToDoList.Server.Test
{
    [TestClass]
    public class AuthorizationContextTest
    {
        private const string UserName = "TESTUSER";
        private const string SystemIdentifier = "Test";
        private const string PlantIdentifier = "Test_Plant";
        private const string CacheKey = UserName + "|" + SystemIdentifier + "|" + PlantIdentifier + "|Equipment=[System.String, id4711, v42]|RecipeManagement=[System.String, id4712, v815]";

        private ICacheManager _cacheManager;
        private ISessionInfo _sessionInfo;
        private IAuthFrontEndService _userSecurityFes;
        private MockRepository _mockRepository;
        private AuthorizationContextData _authContextData;
        private IMethodOptions<string> _systemIdExpectations;
        private IMethodOptions<bool> _isAuthenticatedExpectations;
        private AuthorizationContext _cut;

        [TestInitialize]
        public void SetUp()
        {

            DependencyFactory.RegisterTypeIfMissing<ISessionInfo, SessionInfoMock>();
            DependencyFactory.RegisterTypeIfMissing<IAuthorizationContext, AuthorizationContextMock>();


            _mockRepository = new MockRepository();

            _cacheManager = _mockRepository.DynamicMock<ICacheManager>();

            _userSecurityFes = _mockRepository.DynamicMock<IAuthFrontEndService>();

            _sessionInfo = _mockRepository.DynamicMock<ISessionInfo>();
            Expect.Call(_sessionInfo.UserId).Return(UserName);
      
            Expect.Call(_sessionInfo.Active).Return(true);
            _systemIdExpectations = Expect.Call(_sessionInfo.System).Return(SystemIdentifier);
            _isAuthenticatedExpectations = Expect.Call(_sessionInfo.IsAuthenticated()).Return(true).TentativeReturn(); // Keep this as last expected call in order to allow the return value to be missing.

            DependencyFactory.RenewFactory();
            DependencyFactory.RegisterInstanceIfMissing(() => _sessionInfo);

            _authContextData = new AuthorizationContextData()
                {
                    SecUserAuthorizationInfo = CreateSecUserAuthorizationInfoDto(),
                    UserId = UserName,
                };

            _cut = new AuthorizationContext(_cacheManager, _userSecurityFes);
        }

        /// <summary>
        /// Should reload and cache Auth.Data because of no data in cache
        /// </summary>
        [TestMethod]
        [Owner("duj")]
        public void NoDataInCache()
        {
            Expect.Call(_cacheManager.AddOrGetExisting(
                Arg<Func<AuthorizationContextData>>.Is.Anything,
                Arg<TimeSpan>.Is.Anything,
                Arg.Is(CacheKey),
                Arg<string>.Is.Null))
                .WhenCalled(invocation => invocation.ReturnValue = ((Func<AuthorizationContextData>)invocation.Arguments[0]).Invoke())
                .Return(default(AuthorizationContextData)).TentativeReturn();

            Expect.Call(_userSecurityFes.GetSecAuthorizationInfoList("TESTUSER", true)).Return(_authContextData.SecUserAuthorizationInfo);

            _mockRepository.ReplayAll();

            _cut.LoadAuthorizationData();

            Assert.IsFalse(_cut.HasPermission("UnitTestAction"));

            _mockRepository.VerifyAll();
        }

        ///// <summary>
        ///// Should reload Auth.Data and cause security exception because of user not found
        ///// </summary>
        [TestMethod]
        [Owner("duj")]
        public void UserNotFound()
        {
            _mockRepository.BackToRecord(_sessionInfo, BackToRecordOptions.Expectations);
            Expect.Call(_sessionInfo.Active).Return(true); // Important: If this is not set, the child container will return null for SessionInfo.Current!
            Expect.Call(_sessionInfo.UserId).Return(UserName);
            Expect.Call(_sessionInfo.IsAuthenticated()).Return(false);

            _mockRepository.ReplayAll();

            try
            {
                _cut.LoadAuthorizationData();
                Assert.Fail("Authorization data for user with ID=\"{0}\" should throw SecurityException enclosed in a ServiceException!", _authContextData.UserId);
            }
            catch (Exception srvEx)
            {
                Assert.IsInstanceOfType(srvEx.InnerException, typeof(SecurityException));
            }

            _mockRepository.VerifyAll();
        }

        /// <summary>
        /// Should load Auth.Data from cache and call not the FrontendService
        /// </summary>
        [TestMethod]
        [Owner("duj")]
        public void NotCallFrontendService()
        {
            Expect.Call(_cacheManager.AddOrGetExisting(
                Arg<Func<AuthorizationContextData>>.Is.Anything,
                Arg<TimeSpan>.Is.Anything,
                Arg.Is(CacheKey),
                Arg<string>.Is.Null))
                .Return(_authContextData);

            _mockRepository.ReplayAll();

            _cut.LoadAuthorizationData();

            Assert.IsFalse(_cut.HasPermission("UnitTestAction"));

            _mockRepository.VerifyAll();
        }

        /// <summary>
        /// Should reload and cache Auth.Data because of SystemIdentifier change to Something
        /// </summary>
        [TestMethod]
        [Owner("duj")]
        public void SystemIdentifierChanged()
        {
            var keys = new List<string>();

            Expect.Call(_cacheManager.AddOrGetExisting(
                Arg<Func<AuthorizationContextData>>.Is.Anything,
                Arg<TimeSpan>.Is.Anything,
                Arg<string>.Is.Anything,
                Arg<string>.Is.Null))
                .WhenCalled(invocation => keys.Add((string)invocation.Arguments[2]))
                .Return(_authContextData);

            _systemIdExpectations.Repeat.Once(); // Use default implementation only once.
            Expect.Call(_sessionInfo.System).Return(SystemIdentifier + "1");

            _mockRepository.ReplayAll();

            _cut.LoadAuthorizationData();

            _cut.LoadAuthorizationData();

            Assert.AreEqual(2, keys.Count, "CacheManager should be called twice.");
            Assert.AreNotEqual(keys[0], keys[1], "Cache keys should be different.");

            _mockRepository.VerifyAll();
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
    }
}