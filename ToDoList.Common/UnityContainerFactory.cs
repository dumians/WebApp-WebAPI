// -----------------------------------------------------------------------
// <copyright file="UnityContainerFactory.cs" company="">
// 
// </copyright>
// -----------------------------------------------------------------------


using System;
using System.Diagnostics;
using System.ServiceModel;
using System.Threading;
using System.Web;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;

namespace ToDoList.Common
{
    /// <summary>
    /// Unity Container Factory
    /// </summary>
    public static class UnityContainerFactory
    {
       // private static readonly ILog Log = LogManager.GetLogger(LogCategories.Infrastructure);
        private static readonly ThreadLocal<IUnityContainer> ThreadLocalContext = new ThreadLocal<IUnityContainer>();
        private const string ChildContainerStorageKey = "UnityChildContainerStorageKey";

        /// <summary>
        /// Gets the main container
        /// </summary>
        /// <returns></returns>
        internal static IUnityContainer GetMainContainer()
        {
            return ServiceLocator.Current.GetInstance<IUnityContainer>();
        }

        /// <summary>
        /// Gets the child container
        /// </summary>
        /// <returns></returns>
        internal static IUnityContainer GetChildContainer()
        {
            return GetChildContainerFromCurrentContext();
        }

        /// <summary>
        /// Gets the child container if exists else the main container
        /// </summary>
        /// <returns></returns>
        internal static IUnityContainer GetCurrentContainer()
        {
            return GetChildContainer() ?? GetMainContainer();
        }

        /// <summary>
        /// Creates a new Unity Container and registers it to the Service Locator
        /// </summary>
        /// <returns>Unity Container</returns>
        internal static IUnityContainer CreateAndRegisterMainContainer()
        {
            IUnityContainer mainContainer = new UnityContainer();
            ServiceLocator.SetLocatorProvider(() => new UnityServiceLocator(mainContainer));
            Debug.WriteLine("Main container created and registered to ServiceLocator.");
            return mainContainer;
        }

        /// <summary>
        /// Creates a new Child Container and registers it to the current context
        /// </summary>
        /// <param name="mainContainer"></param>
        internal static IUnityContainer CreateAndRegisterChildContainer(this IUnityContainer mainContainer)
        {
            var childContainer = mainContainer.CreateChildContainer();
            try
            {
                childContainer.RegisterChildContainerToCurrentContext();
                Debug.WriteLine("Child container created and registered to current context.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("Registering child container to current context failed with exception: '{0}'", ex, ex.ToString()));
                childContainer?.Dispose();

                throw;
            }

            return childContainer;
        }

        /// <summary>
        /// Unregisters and disposes the child container
        /// </summary>
        /// <param name="childContainer"></param>
        internal static void DisposeAndUnregisterChildContainer(this IUnityContainer childContainer)
        {
            if (childContainer != null)
            {
                childContainer.UnregisterChildContainerFromCurrentContext();
                childContainer.Dispose();
                Debug.WriteLine("Child container unregistered from current context and disposed.");
            }
        }

        private static void RegisterChildContainerToCurrentContext(this IUnityContainer container)
        {
            if (OperationContext.Current != null)
            {
                if (ChildContainerContext.Current.ChildContainer != null)
                    throw new InvalidOperationException("There is already a child container registered");
                ChildContainerContext.Current.ChildContainer = container;
                Debug.WriteLine("Registered child container to OperationContext");
            }
            else if (HttpContext.Current != null)
            {
                if (HttpContext.Current.Items[ChildContainerStorageKey] != null)
                    throw new InvalidOperationException("There is already a child container registered");
                HttpContext.Current.Items[ChildContainerStorageKey] = container;
                Debug.WriteLine("Registered child container to HttpContext");
            }
            else
            {
                if (ThreadLocalContext.Value != null)
                    throw new InvalidOperationException("There is already a child container registered");
                ThreadLocalContext.Value = container;
                Debug.WriteLine("Registered child container to ThreadLocal");
            }
        }

        private static IUnityContainer GetChildContainerFromCurrentContext()
        {
            IUnityContainer childContainer;
            if (OperationContext.Current != null)
            {
                childContainer = ChildContainerContext.Current.ChildContainer;
            }
            else if (HttpContext.Current != null)
            {
                childContainer = HttpContext.Current.Items[ChildContainerStorageKey] as IUnityContainer;
            }
            else
            {
                childContainer = ThreadLocalContext.Value;
            }

            return childContainer;
        }

        private static void UnregisterChildContainerFromCurrentContext(this IUnityContainer container)
        {
            if (OperationContext.Current != null)
            {
                var childContainer = ChildContainerContext.Current.ChildContainer;
                if (childContainer != null)
                {
                    if (!childContainer.Equals(container))
                        throw new InvalidOperationException("Given container is not the child container");
                    Debug.WriteLine("Unregistered child container from OperationContext");
                    ChildContainerContext.Current.ChildContainer = null;
                }
            }
            else if (HttpContext.Current != null)
            {
                var childContainer = HttpContext.Current.Items[ChildContainerStorageKey] as IUnityContainer;
                if (childContainer != null)
                {
                    if (!childContainer.Equals(container))
                        throw new InvalidOperationException("Given container is not the child container");
                    Debug.WriteLine("Unregistered child container from HttpContext");
                    HttpContext.Current.Items[ChildContainerStorageKey] = null;
                }
            }
            else
            {
                var childContainer = ThreadLocalContext.Value;
                if (childContainer != null)
                {
                    if (!childContainer.Equals(container))
                        throw new InvalidOperationException("Given container is not the child container");
                    Debug.WriteLine("Unregistered child container from ThreadLocal");
                    ThreadLocalContext.Value = null;
                }
            }
        }
    }
}
