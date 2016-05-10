// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DependencyFactory.cs" company="TrivadisAG">
//   Copyright (c) TrivadisAG. All rights reserved.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using Microsoft.Practices.Unity.InterceptionExtension;
using ToDoList.Common.Attributes;

namespace ToDoList.Common
{
    /// <summary>
    /// Simple wrapper for unity resolution.
    /// </summary>
    public class DependencyFactory
    {
      //private static readonly ILog Log = LogManager.GetLogger(LogCategories.Infrastructure);

        public static IInterceptionBehaviourFactory InterceptionBehaviourFactory { get; set; }

        /// <summary>
        /// The one and only instance of the DependencyFactory. There can be only one!
        /// </summary>
        public static DependencyFactory Current { get; protected set; }

        private static readonly object Padlock = new object();

        private bool UnityInterceptionEnabled { get; set; }
        private bool MethodLoggingEnabled { get; set; }
        //   public static bool ExternalContainer;

        static DependencyFactory()
        {
            RenewFactory();
        }

        /// <summary>
        /// Constructor for DependencyFactory which will 
        /// initialize the unity container.
        /// </summary>
        private DependencyFactory()
        {
            var container = UnityContainerFactory.CreateAndRegisterMainContainer();

            UnityInterceptionEnabled = GetUnityInterceptionSetting();
            MethodLoggingEnabled = GetMethodLoggingSetting();

            if (UnityInterceptionEnabled)
            {
                container.AddNewExtension<Interception>();
            }

            var section = (UnityConfigurationSection)ConfigurationManager.GetSection("unity");
            if (section != null)
            {
                section.Configure(container);
            }
        }

        private static IUnityContainer MainContainer
        {
            get { return UnityContainerFactory.GetMainContainer(); }

        }

        private static IUnityContainer ChildContainer
        {
            get { return UnityContainerFactory.GetChildContainer(); }
        }

        private static IUnityContainer CurrentContainer
        {
            get { return UnityContainerFactory.GetCurrentContainer(); }
        }

        private static bool GetUnityInterceptionSetting()
        {
            var unityInterceptionExtension = ConfigurationManager.AppSettings["Service.UnityInterception.Enable"];

            return !string.IsNullOrEmpty(unityInterceptionExtension) && string.Equals(unityInterceptionExtension.ToLower(), "true");
        }

        private static bool GetMethodLoggingSetting()
        {
            if (!GetUnityInterceptionSetting())
            {
                return false;
            }

            var methodLogging = ConfigurationManager.AppSettings["Service.UnityInterception.MethodLogging.Enable"];

            return !string.IsNullOrEmpty(methodLogging) && string.Equals(methodLogging.ToLower(), "true");
        }

        /// <summary>
        /// Creates a new factory including a new container.
        /// This should only be used for testing purposes!
        /// </summary>
        public static void RenewFactory()
        {
            //  if (ExternalContainer) return;
            lock (Padlock)
            {
                if (Current != null)
                {
                    MainContainer.Dispose();
                    Current = null;
                }
                Current = new DependencyFactory();
            }
        }

        //public static void RenewFactory(IUnityContainer rootContainer)
        //{
        //    if (ExternalContainer) return;
        //    lock (Padlock)
        //    {
        //        if (Current != null && !ExternalContainer)
        //        {
        //            MainContainer.Dispose();
        //            Current = null;
        //        }
        //        ExternalContainer = true;
        //        MainContainer = rootContainer;
        //        Current = new DependencyFactory();
        //    }
        //}


        /// <summary>
        /// Creates a child container in the main container
        /// </summary>
        public static void CreateChildContainer()
        {
            if (ChildContainer == null)
            {
                MainContainer.CreateAndRegisterChildContainer();
            }
            else
            {
                throw new InvalidOperationException("ChildContainer already exists");
            }
        }

        /// <summary>
        /// Disposes the child container
        /// </summary>
        public static void DisposeChildContainer()
        {
            ChildContainer.DisposeAndUnregisterChildContainer();
        }

        public static bool ChildContainerExists()
        {
            return ChildContainer != null;
        }

        /// <summary>
        /// Resolves the type parameter T to an instance of the appropriate type (and alias).
        /// </summary>
        /// <param name="alias">alias name</param>
        /// <typeparam name="T">Type of object to return</typeparam>
        public static T Resolve<T>(string alias = null)
        {
            return alias == null ? CurrentContainer.Resolve<T>() : CurrentContainer.Resolve<T>(alias);
        }

        /// <summary>
        /// Resolves the type parameter T to an instance of the appropriate type (and alias).
        /// </summary>
        /// <param name="alias">alias name</param>
        /// <typeparam name="T">Type of object to return</typeparam>
        public static T ResolveSafe<T>(string alias = null)
        {
            var ret = default(T);

            try
            {
                ret = Resolve<T>(alias);
            }
            catch (Exception ex)
            {
                if (string.IsNullOrEmpty(alias))
                    Debug.WriteLine(String.Format("DependencyFactory ResolveSafe unable to resolve type {0}", ex, typeof(T)));
            }

            return ret;
        }

        /// <summary>
        ///  Resolves the given type to an appropriate instance.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object Resolve(Type type)
        {
            return CurrentContainer.Resolve(type);
        }

        /// <summary>
        /// Resolves all instances of the given type from the container.
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <returns>Instances of given type</returns>
        public static IEnumerable<T> ResolveAll<T>()
        {
            return CurrentContainer.ResolveAll<T>();
        }

        /// <summary>
        /// Determines if the type (with the given alias) is registered with the unity container
        /// </summary>
        /// <param name="alias">alias</param>
        /// <typeparam name="T">Type</typeparam>
        /// <returns>true if type is registered, otherwise false</returns>
        // [FS]: [Pure]
        public static bool IsRegistered<T>(string alias = null)
        {
            return alias == null ? CurrentContainer.IsRegistered(typeof(T)) : CurrentContainer.IsRegistered(typeof(T), alias);
        }

        /// <summary>
        /// Registers a type in the main container only if that type was not already registered.
        /// </summary>
        /// <param name="registerAsSingleton">Registers the type as a singleton.</param>
        /// <param name="ctorParam">parameters to pass to the ctor of the registered type; null to use default selection algorithm</param>
        public static void RegisterTypeIfMissing<TFrom, TTo>(bool registerAsSingleton = true, object[] ctorParam = null)
        {
            RegisterTypeIfMissingInternal(typeof(TFrom), typeof(TTo), MainContainer, false, registerAsSingleton, null, ctorParam);
        }

        /// <summary>
        /// Registers a type in the child container only if that type was not already registered.
        /// </summary>
        /// <param name="registerAsSingleton">Registers the type as a singleton.</param>
        /// <param name="ctorParam">parameters to pass to the ctor of the registered type; null to use default selection algorithm</param>
        public static void RegisterTypeIfMissingInChildContainer<TFrom, TTo>(bool registerAsSingleton = true, object[] ctorParam = null)
        {
            RegisterTypeIfMissingInternal(typeof(TFrom), typeof(TTo), ChildContainer, false, registerAsSingleton, null, ctorParam);
        }

        /// <summary>
        /// Registers a type in the main container only if that type was not already registered.
        /// </summary>
        /// <param name="registerAsSingleton">Registers the type as a singleton.</param>
        /// <param name="alias">Alias for the registration</param>
        /// <param name="ctorParam">parameters to pass to the ctor of the registered type; null to use default selection algorithm</param>
        public static void RegisterTypeIfMissing<TFrom, TTo>(bool registerAsSingleton, string alias, object[] ctorParam = null)
        {
            //if (string.IsNullOrEmpty(alias))
            //    throw new ArgumentException("alias must not be null or empty");
            //Contract.EndContractBlock();
            // [FS]
           
            RegisterTypeIfMissingInternal(typeof(TFrom), typeof(TTo), MainContainer, false, registerAsSingleton, alias, ctorParam);
        }

        /// <summary>
        /// Registers a type in the child container only if that type was not already registered.
        /// </summary>
        /// <param name="registerAsSingleton">Registers the type as a singleton.</param>
        /// <param name="alias">Alias for the registration</param>
        /// <param name="ctorParam">parameters to pass to the ctor of the registered type; null to use default selection algorithm</param>
        public static void RegisterTypeIfMissingInChildContainer<TFrom, TTo>(bool registerAsSingleton, string alias, object[] ctorParam = null)
        {
            //if (string.IsNullOrEmpty(alias))
            //    throw new ArgumentException("alias must not be null or empty");
            //Contract.EndContractBlock();
            // [FS]
          
            RegisterTypeIfMissingInternal(typeof(TFrom), typeof(TTo), ChildContainer, false, registerAsSingleton, alias, ctorParam);
        }

        /// <summary>
        /// Registers a type in the main container with hierachical lifetime manager only if that type was not already registered.
        /// </summary>
        /// <param name="alias">Alias for the registration</param>
        /// <param name="ctorParam">parameters to pass to the ctor of the registered type; null to use default selection algorithm</param>
        public static void RegisterTypeIfMissingHierachical<TFrom, TTo>(string alias = null, object[] ctorParam = null)
        {
            RegisterTypeIfMissingInternal(typeof(TFrom), typeof(TTo), MainContainer, true, false, alias, ctorParam);
        }

        /// <summary>
        /// Registers a type in the main container only if that type was not already registered.
        /// </summary>
        /// <param name="fromType">The interface type to register.</param>
        /// <param name="toType">The type implementing the interface.</param>
        /// <param name="registerAsSingleton">Registers the type as a singleton.</param>
        /// <param name="alias">the alias to register the type</param>
        /// <param name="ctorParam">parameters to pass to the ctor of the registered type; null to use default selection algorithm</param>
        public static void RegisterTypeIfMissing(Type fromType, Type toType, bool registerAsSingleton, string alias = null, object[] ctorParam = null)
        {
            RegisterTypeIfMissingInternal(fromType, toType, MainContainer, false, registerAsSingleton, alias, ctorParam);
        }

        /// <summary>
        /// Registers a type in the child container only if that type was not already registered.
        /// </summary>
        /// <param name="fromType">The interface type to register.</param>
        /// <param name="toType">The type implementing the interface.</param>
        /// <param name="registerAsSingleton">Registers the type as a singleton.</param>
        /// <param name="alias">the alias to register the type</param>
        /// <param name="ctorParam">parameters to pass to the ctor of the registered type; null to use default selection algorithm</param>
        [Obsolete]
        public static void RegisterTypeIfMissingInChildContainer(Type fromType, Type toType, bool registerAsSingleton, string alias = null, object[] ctorParam = null)
        {
            RegisterTypeIfMissingInternal(fromType, toType, ChildContainer, false, registerAsSingleton, alias, ctorParam);
        }

        /// <summary>
        /// Registers a type in the main container with hierachical lifetime manager only if that type was not already registered.
        /// </summary>
        /// <param name="fromType">The interface type to register.</param>
        /// <param name="toType">The type implementing the interface.</param>
        /// <param name="alias">the alias to register the type</param>
        /// <param name="ctorParam">parameters to pass to the ctor of the registered type; null to use default selection algorithm</param>
        public static void RegisterTypeIfMissingHierachical(Type fromType, Type toType, string alias = null, object[] ctorParam = null)
        {
            RegisterTypeIfMissingInternal(fromType, toType, MainContainer, true, false, alias, ctorParam);
        }

        private static void RegisterTypeIfMissingInternal(
            Type fromType, 
            Type toType, 
            IUnityContainer container, 
            bool registerHierachical, 
            bool registerAsSingleton,
            string alias = null, 
            object[] ctorParam = null)
        {
            if (fromType == null)
            {
                throw new ArgumentNullException("fromType");
            }
            if (toType == null)
            {
                throw new ArgumentNullException("toType");
            }
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }



            Debug.WriteLine(string.Format("RegisterTypeIfMissing fromType: {0} toType: {1} registerAsSingleton: {2} alias: {3}", fromType, toType, registerAsSingleton.ToString(CultureInfo.InvariantCulture), alias));

            LifetimeManager lifetimeManager = null;
            if (registerHierachical)
            {
                lifetimeManager = new HierarchicalLifetimeManager();
            }
            else if (registerAsSingleton)
            {
                lifetimeManager = new ContainerControlledLifetimeManager();
            }

            if (alias == null && !container.IsRegistered(fromType))
            {
                ConfigureDefaultInterceptionBehavior(fromType, container);

                RegisterType(fromType, toType, container, null, lifetimeManager, ctorParam);
            }
            else if (alias != null && !container.IsRegistered(fromType, alias))
            {
                if (!container.IsRegistered(fromType))
                {
                    ConfigureDefaultInterceptionBehavior(fromType, container, alias);
                }

                RegisterType(fromType, toType, container, alias, lifetimeManager, ctorParam);
            }
        }

        private static void RegisterType(
            Type fromType,
            Type toType,
            IUnityContainer container,
            string alias,
            LifetimeManager lifetimeManager,
            object[] ctorParam)
        {
            InjectionConstructor injectCtor = null;
            if (ctorParam != null)
            {
                injectCtor = new InjectionConstructor(ctorParam);
            }

            if (injectCtor == null)
            {
                if (lifetimeManager == null)
                {
                    container.RegisterType(fromType, toType, alias);
                }
                else
                {
                    container.RegisterType(fromType, toType, alias, lifetimeManager);
                }
            }
            else
            {
                if (lifetimeManager == null)
                {
                    container.RegisterType(fromType, toType, alias, injectCtor);
                }
                else
                {
                    container.RegisterType(fromType, toType, alias, lifetimeManager, injectCtor);
                }
            }
        }

        /// <summary>
        /// Register an instance in the main container
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <param name="instanceName"></param>
        /// <param name="instance"></param>
        /// <param name="registerAsSingleton"></param>
        public static void RegisterInstanceIfMissing<TInterface>(Func<TInterface> instance, string instanceName = null, bool registerAsSingleton = true)
        {
            RegisterInstanceIfMissingInternal(instance, MainContainer, instanceName, registerAsSingleton);
        }

        /// <summary>
        /// Register an instance in the child container
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <param name="instanceName"></param>
        /// <param name="instance"></param>
        /// <param name="registerAsSingleton"></param>
        [Obsolete]
        public static void RegisterInstanceIfMissingInChildContainer<TInterface>(Func<TInterface> instance, string instanceName = null, bool registerAsSingleton = true)
        {
            RegisterInstanceIfMissingInternal(instance, ChildContainer, instanceName, registerAsSingleton);
        }

        private static void RegisterInstanceIfMissingInternal<TInterface>(Func<TInterface> instance, IUnityContainer container, string instanceName = null, bool registerAsSingleton = true)
        {
           
            Debug.WriteLine(string.Format("RegisterInstanceIfMissing toType: {0} instance name: {1} registerAsSingleton: {2}", instance.GetType(), instanceName, registerAsSingleton.ToString(CultureInfo.InvariantCulture)));

            if (instanceName == null && !container.IsRegistered<TInterface>())
            {
                ConfigureDefaultInterceptionBehavior<TInterface>(container);

                if (registerAsSingleton)
                {
                    container.RegisterInstance(instance(), new ContainerControlledLifetimeManager());
                }
                else
                {
                    container.RegisterInstance(instance());
                }
            }
            if (instanceName != null && !container.IsRegistered<TInterface>(instanceName))
            {
                if (!container.IsRegistered<TInterface>())
                {
                    ConfigureDefaultInterceptionBehavior<TInterface>(container, instanceName);
                }

                if (registerAsSingleton)
                {
                    container.RegisterInstance(instanceName, instance(), new ContainerControlledLifetimeManager());
                }
                else
                {
                    container.RegisterInstance(instanceName, instance());
                }
            }
        }

        /// <summary>
        /// Register an instance in the main container externally controlled
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <param name="instanceName"></param>
        /// <param name="instance"></param>
        public static void RegisterInstanceIfMissingExtControlled<TInterface>(Func<TInterface> instance, string instanceName = null)
        {
            RegisterInstanceIfMissingExtControlledInternal(instance, MainContainer, instanceName);
        }

        /// <summary>
        /// Register an instance in the child container externally controlled
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <param name="instanceName"></param>
        /// <param name="instance"></param>
        public static void RegisterInstanceIfMissingExtControlledInChildContainer<TInterface>(Func<TInterface> instance, string instanceName = null)
        {
            RegisterInstanceIfMissingExtControlledInternal(instance, ChildContainer, instanceName);
        }

        private static void RegisterInstanceIfMissingExtControlledInternal<TInterface>(Func<TInterface> instance, IUnityContainer container, string instanceName = null)
        {
            //if (instance == null)
            //{
            //    throw new ArgumentNullException("instance");
            //}
            //if (container == null)
            //{
            //    throw new ArgumentNullException("container");
            //}
            //Contract.EndContractBlock();
            // [FS]
            
            if (instanceName == null && !container.IsRegistered<TInterface>())
            {
                ConfigureDefaultInterceptionBehavior<TInterface>(container);
                container.RegisterInstance(instance(), new ExternallyControlledLifetimeManager());
            }
            if (instanceName != null && !container.IsRegistered<TInterface>(instanceName))
            {
                if (!container.IsRegistered<TInterface>())
                {
                    ConfigureDefaultInterceptionBehavior<TInterface>(container, instanceName);
                }

                container.RegisterInstance(instanceName, instance(), new ExternallyControlledLifetimeManager());
            }
        }

        private static void ConfigureDefaultInterceptionBehavior<TInterface>(IUnityContainer container, string alias = null)
        {
            ConfigureDefaultInterceptionBehavior(typeof(TInterface), container, alias);
        }

        private static void ConfigureDefaultInterceptionBehavior(Type interfaceType, IUnityContainer container, string alias = null)
        {
            // Just configure a Default Interception Behavior if registration occurs with an interface otherwise an "... not interceptable exception ..." occurs.
            if (interfaceType.IsInterface)
            {
                var interceptorBehaviour = CreateInterceptionBehaviours(interfaceType);

                if (interceptorBehaviour.Any())
                {
                    if (alias == null)
                    {
                        container.RegisterType(interfaceType, interceptorBehaviour.ToArray());
                    }
                    else
                    {
                        container.RegisterType(interfaceType, alias, interceptorBehaviour.ToArray());
                    }
                }
                else
                {
                    if (alias == null)
                    {
                        container.RegisterType(interfaceType);
                    }
                    else
                    {
                        container.RegisterType(interfaceType, alias);
                    }
                }
            }
            else
            {
                
                    Debug.WriteLine("No DefaultInterceptionBehavior for type " + interfaceType + " added because it is no interface");
               
            }
        }

        private static IEnumerable<InjectionMember> CreateInterceptionBehaviours(Type interfaceType)
        {
            var result = new List<InjectionMember>();

            if (Current.UnityInterceptionEnabled)
            {
                var skipAttrs = ReflectionUtils.GetAttributes<SkipInterceptionAttribute>(interfaceType);
                if (skipAttrs == null || !skipAttrs.Any())
                {
                    if (InterceptionBehaviourFactory != null)
                    {
                        result.AddRange(InterceptionBehaviourFactory.CreateInterceptionBehaviours(interfaceType));
                    }

                    //if (Current.MethodLoggingEnabled)
                    //{
                    //    result.Add(new DefaultInterceptionBehavior<MethodLoggingBehaviour>());
                    //}

                    if (result.Any())
                    {
                        // some behaviours => add an interceptor as first element
                        result.Insert(0, new DefaultInterceptor(new InterfaceInterceptor()));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Checks all dependencies in the current container if they are referencing a hierarchical registered (i.e. request specific)
        /// type.
        /// </summary>
        public static void CheckContainerForReferencedHierarchicalRegisteredTypes()
        {
            var hierarchicalRegisteredTypes =
                CurrentContainer.Registrations.Where(r => r.LifetimeManagerType == typeof(HierarchicalLifetimeManager)).Select(r => r.RegisteredType);

            // get registrations referencing a hierarchical registered type
            var suspiciousRegistrations =
                CurrentContainer.Registrations.Where(
                    r =>
                        ReflectionUtils.GetDependencyFields(r.MappedToType,
                            ReflectionUtils.GetReferenceMembers(r.MappedToType))
                            .Select(f => (f.MemberType == MemberTypes.Field ? ((FieldInfo)f).FieldType : ((PropertyInfo)f).PropertyType))
                            .Intersect(hierarchicalRegisteredTypes)
                            .Any()).ToList();

            if (!suspiciousRegistrations.Any()) return;

            // convert to dictionary (referencing type -> list of field types)
            var suspiciusReferencesByType = suspiciousRegistrations.ToDictionary(reg => reg.MappedToType, reg => ReflectionUtils.GetDependencyFields(reg.MappedToType,
                ReflectionUtils.GetReferenceMembers(reg.MappedToType))
                .Select(f => (f.MemberType == MemberTypes.Field ? ((FieldInfo)f).FieldType : ((PropertyInfo)f).PropertyType))
                .Intersect(hierarchicalRegisteredTypes));

            // create message
            var sb = new StringBuilder("Reference(s) to hierarchical registered type(s): ");
            foreach (var classRefTypes in suspiciusReferencesByType)
            {
                sb.Append("(")
                    .Append(classRefTypes.Key.Name)
                    .Append(" -> ");
                classRefTypes.Value.Select(t => t.Name)
                    .Aggregate(sb, (ag, n) => ag.Append(n).Append(", "));
                sb.Append("), ");
            }
           Debug.WriteLine(sb);

            throw new ArgumentException(sb.ToString());
        }

    }
}
