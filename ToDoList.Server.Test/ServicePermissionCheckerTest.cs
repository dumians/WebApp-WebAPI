using System;
using System.Collections;
using System.Linq;
using System.ServiceModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToDoList.Common.Attributes;
using ToDoList.Server.Common.PermisionsChecker;

namespace ToDoList.Server.Test
{
    [TestClass]
    public class ServicePermissionCheckerTest
    {
        const string PN_METH_WO_NAME = "HugoPermission";
        const string PN_METH_WITH_NAME_1 = "Egon1Permission";
        const string PN_METH_WITH_NAME_2 = "Egon2Permission";

        private ServicePermissionChecker _sut;

        [TestInitialize]
        public void SetUp()
        {
            _sut = new ServicePermissionChecker();
        }

        [TestMethod]
        [Owner("duj")]
        public void GetWcfPermissionsFromServiceInterfaceUnknownAction()
        {
            var permissions = _sut.GetWcfServicePermissions(typeof(ITestService), "unknownAction");

            Assert.IsFalse(permissions.Any());
        }

        [TestMethod]
        [Owner("duj")]
        public void GetWcfPermissionsFromServiceInterfaceWithoutName()
        {
            var permissions = _sut.GetWcfServicePermissions(typeof(ITestService), "testMethodWithoutName");

            Assert.IsTrue(permissions.Any());
            Assert.AreEqual(1, permissions.Count());
            Assert.AreEqual(PN_METH_WO_NAME, permissions.First().Permission);
        }

        [TestMethod]
        [Owner("duj")]
        public void GetWcfPermissionsFromServiceClassWithoutName()
        {
            var permissions = _sut.GetWcfServicePermissions(typeof(TestService), "testMethodWithoutName");

            Assert.IsTrue(permissions.Any());
            Assert.AreEqual(1, permissions.Count());
            Assert.AreEqual(PN_METH_WO_NAME, permissions.First().Permission);
        }

        [TestMethod]
        [Owner("duj")]
        public void GetWcfPermissionsFromServiceInterfaceWithName()
        {
            var permissions = _sut.GetWcfServicePermissions(typeof(ITestService), "NameOfTestMethod");

            Assert.IsTrue(permissions.Any());
            Assert.AreEqual(2, permissions.Count());
            Assert.IsTrue(permissions.Select(perm => perm.Permission).Contains(PN_METH_WITH_NAME_1));
            Assert.IsTrue(permissions.Select(perm => perm.Permission).Contains(PN_METH_WITH_NAME_2));
        }

        [TestMethod]
        [Owner("duj")]
        public void GetWcfPermissionsFromServiceClassWithName()
        {
            var permissions = _sut.GetWcfServicePermissions(typeof(TestService), "NameOfTestMethod");

            Assert.IsTrue(permissions.Any());
            Assert.AreEqual(2, permissions.Count());
            Assert.IsTrue(permissions.Select(perm => perm.Permission).Contains(PN_METH_WITH_NAME_1));
            Assert.IsTrue(permissions.Select(perm => perm.Permission).Contains(PN_METH_WITH_NAME_2));
        }

        [TestMethod]
        [Owner("duj")]
        public void GetMethodPermissionsFromServiceInterfaceNoPermission()
        {
            var permissions = _sut.GetServiceMethodPermissions(typeof(INonWcfTestService).GetMethods().Where(meth => string.Equals(meth.Name, "testMethodWithoutPermissions")));

            Assert.IsFalse(permissions.Any());
        }

        [TestMethod]
        [Owner("duj")]
        public void GetMethodPermissionsFromServiceInterfaceOnePermission()
        {
            var permissions = _sut.GetServiceMethodPermissions(typeof(INonWcfTestService).GetMethods().Where(meth => string.Equals(meth.Name, "testMethodWithoutName")));

            Assert.IsTrue(permissions.Any());
            Assert.AreEqual(1, permissions.Count());
            Assert.AreEqual(PN_METH_WO_NAME, permissions.First().Permission);
        }

        [TestMethod]
        [Owner("duj")]
        public void GetMethodPermissionsFromServiceInterfaceTwoPermissions()
        {
            var permissions = _sut.GetServiceMethodPermissions(typeof(INonWcfTestService).GetMethods().Where(meth => string.Equals(meth.Name, "testMethodWithName")));

            Assert.IsTrue(permissions.Any());
            Assert.AreEqual(2, permissions.Count());
            Assert.IsTrue(permissions.Select(perm => perm.Permission).Contains(PN_METH_WITH_NAME_1));
            Assert.IsTrue(permissions.Select(perm => perm.Permission).Contains(PN_METH_WITH_NAME_2));
        }

        [TestMethod]
        [Owner("duj")]
        public void GetMethodPermissionsFromServiceClassNoPermission()
        {
            var permissions = _sut.GetServiceMethodPermissions(typeof(NonWcfTestService).GetMethods().Where(meth => string.Equals(meth.Name, "testMethodWithoutPermissions")));

            Assert.IsFalse(permissions.Any());
        }

        [TestMethod]
        [Owner("duj")]
        public void GetMethodPermissionsFromServiceClassOnePermission()
        {
            var permissions = _sut.GetServiceMethodPermissions(typeof(NonWcfTestService).GetMethods().Where(meth => string.Equals(meth.Name, "testMethodWithoutName")));

            Assert.IsTrue(permissions.Any());
            Assert.AreEqual(1, permissions.Count());
            Assert.AreEqual(PN_METH_WO_NAME, permissions.First().Permission);
        }

        [TestMethod]
        [Owner("duj")]
        public void GetMethodPermissionsFromServiceClassTwoPermissions()
        {
            var permissions = _sut.GetServiceMethodPermissions(typeof(NonWcfTestService).GetMethods().Where(meth => string.Equals(meth.Name, "testMethodWithName")));

            Assert.IsTrue(permissions.Any());
            Assert.AreEqual(2, permissions.Count());
            Assert.IsTrue(permissions.Select(perm => perm.Permission).Contains(PN_METH_WITH_NAME_1));
            Assert.IsTrue(permissions.Select(perm => perm.Permission).Contains(PN_METH_WITH_NAME_2));
        }

        [TestMethod]
        [Owner("duj")]
        public void CheckHasPermissionOfInterfaceNoPermission()
        {
            var hasPermission = _sut.HasServicePermission(typeof(IEnumerable));

            Assert.IsFalse(hasPermission);
        }

        [TestMethod]
        [Owner("duj")]
        public void CheckHasPermissionOfClassNoPermission()
        {
            var hasPermission = _sut.HasServicePermission(typeof(String));

            Assert.IsFalse(hasPermission);
        }

        [TestMethod]
        [Owner("duj")]
        public void CheckHasPermissionOfInterfaceWithPermission()
        {
            var hasPermission = _sut.HasServicePermission(typeof(ITestService));

            Assert.IsTrue(hasPermission);
        }

        [TestMethod]
        [Owner("duj")]
        public void CheckHasPermissionOfClassWithPermission()
        {
            var hasPermission = _sut.HasServicePermission(typeof(ClassWithPermissionOnMethod));

            Assert.IsTrue(hasPermission);
        }

        [ServiceContract(Namespace = "none")]
        private interface ITestService
        {
            [OperationContract]
            [ServicePermission(PN_METH_WO_NAME)]
            void testMethodWithoutName();

            [OperationContract(Name = "NameOfTestMethod")]
            [ServicePermission(PN_METH_WITH_NAME_1)]
            [ServicePermission(PN_METH_WITH_NAME_2)]
            void testMethodWithName();
        }

        private class TestService : ITestService
        {
            public void testMethodWithoutName()
            {
            }

            public void testMethodWithName()
            {
            }
        }

        private interface INonWcfTestService
        {
            [ServicePermission(PN_METH_WO_NAME)]
            void testMethodWithoutName();

            [ServicePermission(PN_METH_WITH_NAME_1)]
            [ServicePermission(PN_METH_WITH_NAME_2)]
            void testMethodWithName();

            void testMethodWithoutPermissions();
        }

        private class NonWcfTestService : INonWcfTestService
        {
            public void testMethodWithoutName()
            {
            }

            public void testMethodWithName()
            {
            }

            public void testMethodWithoutPermissions()
            {
            }
        }

        private class ClassWithPermissionOnMethod
        {
            [ServicePermission(PN_METH_WITH_NAME_1)]
            public void SomeOp()
            {
            }
        }
    }
}