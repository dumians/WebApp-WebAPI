using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.Mocks;
using ToDoList.Common;
using ToDoList.Common.Attributes;
using ToDoList.Common.SessionInfo;
using ToDoList.Server.Common.PermisionsChecker;
using ToDoList.Server.Common.ServerCallContext;
using ToDoList.Server.Common.Wcf;

namespace ToDoList.Server.Test
{
    /// <summary>
    /// A helper class that provides methods needed to easily implement unit tests for checking the evaluation of the permissions
    /// defined for methods (<see cref="ServicePermissionAttribute"/>) of service interfaces using the complete call chain:
    /// ServiceInteface -> DI resolve -> <see cref="ServicePermissionCheckingBehaviour"/> -> <see cref="ServicePermissionChecker"/> -> <see cref="AuthorizationContext"/>
    /// </summary>
    /// 
    /// <remarks>Before using the static methods of this helper, make sure the <see cref="DependencyFactory.InterceptionBehaviourFactory"/> is set (<see cref="ServerInterceptionBehaviourFactory"/>)
    /// and the following configuration parameters are set:
    /// <code>"Service.UnityInterception.Enable" = "true"</code>
    /// <code>"Service.UnityInterception.MethodLogging.Enable" = "false"</code>
    /// </remarks>
    /// 
    /// <seealso cref="ServicePermissionCheckingBehaviourTest"/>
    public class ServicePermissionTestHelper
    {
        /// <summary>
        /// Hidden constructor for helper class. No instance is needed because every provided method is static.
        /// </summary>
        private ServicePermissionTestHelper()
        {
        }

        /// <summary>
        /// Tests the permission evaluation for the specified method of the given service interface using a variable number of permission na
        /// </summary>
        /// <typeparam name="T">The interface type to check the given method for.</typeparam>
        /// <param name="checkedMethod">The method in the given interface type to check.</param>
        /// <param name="grantPermission">The permission flag which can be used to simulate the result of the user permission evalutation.</param>
        /// <param name="expectedPermissionNames">The permission names expected to be found as attributes (<see cref="ServicePermissionAttribute"/>) on the given interface method.</param>
        /// 
        /// <remarks>Note that calling this method removes all registered types/instances from the <see cref="DependencyFactory"/>!</remarks>
        public static void TestPermissionChecks<T>(Action<T> checkedMethod, bool grantPermission, params string[] expectedPermissionNames)
        {
            TestPermissionChecks<T>(checkedMethod, grantPermission, expectedPermissionNames.ToList());
        }

        /// <summary>
        /// Tests the permission evaluation for the specified method of the given service interface using the given enumeration of permission na
        /// </summary>
        /// <typeparam name="T">The interface type to check the given method for.</typeparam>
        /// <param name="checkedMethod">The method in the given interface type to check.</param>
        /// <param name="grantPermission">The permission flag which can be used to simulate the result of the user permission evalutation.</param>
        /// <param name="expectedPermissionNames">An enumeration of the permission names expected to be found as attributes (<see cref="ServicePermissionAttribute"/>) on the given interface method.</param>
        /// 
        /// <remarks>Note that calling this method removes all registered types/instances from the <see cref="DependencyFactory"/>!</remarks>
        public static void TestPermissionChecks<T>(Action<T> checkedMethod, bool grantPermission, IEnumerable<string> expectedPermissionNames)
        {
            DependencyFactory.RenewFactory();

            DependencyFactory.RegisterTypeIfMissing<ISessionInfo, SessionInfoMock>();
            DependencyFactory.RegisterInstanceIfMissing<IAuthorizationContext>(() => MockRepository.GenerateStub<IAuthorizationContext>());
            DependencyFactory.RegisterTypeIfMissingHierachical<ITrustedScope, TrustedScope>();

            var authCxt = DependencyFactory.Resolve<IAuthorizationContext>();

            // reset stubs
            authCxt.BackToRecord(BackToRecordOptions.All);
            authCxt.Replay();

            authCxt.Stub(x => x.HasPermission(Arg<string>.Matches(a => expectedPermissionNames.Contains(a)))).Return(grantPermission);
            authCxt.Stub(x => x.HasPermission(Arg<string>.Matches(a => !expectedPermissionNames.Contains(a)))).Throw(new InvalidOperationException("Unexpected permission!"));

            // mock the service
            DependencyFactory.RegisterInstanceIfMissing<T>(() => (T)MockRepository.GenerateStub(typeof(T)));

            // call given method on the mocked service
            checkedMethod.Invoke(DependencyFactory.Resolve<T>());

            authCxt.AssertWasCalled(x => x.HasPermission(Arg<string>.Matches(a => expectedPermissionNames.Contains(a))));
        }
    }
}