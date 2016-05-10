using System;
using System.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToDoList.Common;
using ToDoList.Common.Attributes;
using ToDoList.Server.Common.Wcf;

namespace ToDoList.Server.Test
{
    /// <summary>
    /// A reference unit test implementation for evaluating the permissions of methods in a service interface.
    /// The <see cref="ServicePermissionTestHelper"/> class is used to take care of both the initialization and the processing of the service mock permission evaluation.
    /// </summary>
    [TestClass]
    public class ServicePermissionCheckingBehaviourTest
    {
        const string PN_PERMISSION_1 = "Permission1";
        const string PN_PERMISSION_2 = "Permission2";
        const string PN_NO_PERMISSION = "NoPermission";
         private IInterceptionBehaviourFactory oldfactory ;

        [TestInitialize]
        public void TestInitialize()
        {
            ConfigurationManager.AppSettings["Service.UnityInterception.Enable"] = "true";
            ConfigurationManager.AppSettings["Service.UnityInterception.MethodLogging.Enable"] = "false";
            oldfactory = DependencyFactory.InterceptionBehaviourFactory;
            DependencyFactory.InterceptionBehaviourFactory = new ServerInterceptionBehaviourFactory();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            //set back the old value
            DependencyFactory.InterceptionBehaviourFactory = oldfactory;
        }

        [TestMethod]
        [Owner("duj")]
        public void TestCheckExistingServicePermissionGranted()
        {
            ServicePermissionTestHelper.TestPermissionChecks<ITestService>(x => x.TestMethodWithoutName(), true, PN_PERMISSION_1, PN_PERMISSION_2);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        [Owner("duj")]
        public void TestCheckExistingServicePermissionDenied()
        {
            ServicePermissionTestHelper.TestPermissionChecks<ITestService>(x => x.TestMethodWithoutName(), false, PN_PERMISSION_1, PN_PERMISSION_2);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        [Owner("duj")]
        public void TestCheckNonExistingServicePermissionGranted()
        {
            ServicePermissionTestHelper.TestPermissionChecks<ITestService>(x => x.TestMethodWithoutName(), true, PN_NO_PERMISSION);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        [Owner("duj")]
        public void TestCheckNonExistingServicePermissionDenied()
        {
            ServicePermissionTestHelper.TestPermissionChecks<ITestService>(x => x.TestMethodWithoutName(), false, PN_NO_PERMISSION);
        }

        /// <summary>
        /// A little dummy service interface to evaluate the service permissions for.
        /// </summary>
        public interface ITestService
        {
            [ServicePermission(PN_PERMISSION_1)]
            [ServicePermission(PN_PERMISSION_2)]
            void TestMethodWithoutName();
        }
    }
}
