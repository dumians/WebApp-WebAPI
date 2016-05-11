using Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;

namespace TodoListWcfService.App_Code
{
    // <summary>
    /// The AppInitializer
    /// </summary>
    public static class AppInitializer
    {

        /// <summary>
        /// The Logger
        /// </summary>
        private static ILog _log = LogManager.GetLogger(LogCategories.Infrastructure);

        /// <summary>
        ///  intialize web host
        /// </summary>
        public static void AppInitialize()
        {
            //start-up correlation id

            Trace.CorrelationManager.StartLogicalOperation();
            Trace.CorrelationManager.ActivityId = Guid.NewGuid();

            AppDomain.CurrentDomain.UnhandledException += AppDomainUnhandledException;
            AppDomain.CurrentDomain.DomainUnload += AppDomainUnload;
            AppDomain.CurrentDomain.ProcessExit += AppDomainProcessExit;

#if (DEBUG)
            //On debuging scenarios with attached debuger rise an write lock expetion on enliblogger - use the Debugviewr to sse the first chance expetions
            //ILog exceptionLog = LogManager.GetLogger(LogCategories.FirstChanceExceptions);
            AppDomain.CurrentDomain.FirstChanceException += (source, e) => Debug.WriteLine("FirstChanceException event raised in {0}: {1} {2}", AppDomain.CurrentDomain.FriendlyName, GetFirstaChanceExCaller(e.Exception), e.Exception);

      
#endif

            InitializeDependencyFactory();
           

#if (DEBUG)
            DependencyFactory.CheckContainerForReferencedHierarchicalRegisteredTypes();
#endif
        }

        /// <summary>
        /// On AppDomainUnload-Action
        /// </summary>
        /// <param name="sender">The Sender</param>
        /// <param name="e">The EventArgs</param>
        private static void AppDomainUnload(object sender, EventArgs e)
        {
            Debug.WriteLine("AppDomainUnload handled on main host thread");

        }

        /// <summary>
        /// The AppDomainProcessExit-Action
        /// </summary>
        /// <param name="sender">The Sender</param>
        /// <param name="e">The EventArgs</param>
        private static void AppDomainProcessExit(object sender, EventArgs e)
        {
            Debug.WriteLine(">>> Application Shutting down ... Dispose Unity Main Container");
        }

        /// <summary>
        /// The AppDomainUnhandledException-Action
        /// </summary>
        /// <param name="sender">The Sender</param>
        /// <param name="e">The EventArgs</param>
        private static void AppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            LogException(ex);

            if (e.IsTerminating)
            {
                Debug.WriteLine("Application Shutdown due to unhandled exception in a thread", ex);
            }
        }

        /// <summary>
        /// Get FirstaChanceExeption-Caller
        /// </summary>
        /// <param name="ex">The Exception</param>
        /// <returns>The Caller of the Exception</returns>
        private static string GetFirstaChanceExCaller(Exception ex)
        {
            var stackTrace = new StackTrace(ex);
            var sourceFrame = stackTrace.GetFrame(0);
            var throwingMethod = sourceFrame.GetMethod();

            var sourceAssembly = throwingMethod != null && throwingMethod.DeclaringType != null ? throwingMethod.DeclaringType.Assembly : null;
            var assemblyName = sourceAssembly != null ? sourceAssembly.GetName().Name : null;

            return string.Format("First Chance {0} method {1} line {2}", assemblyName, throwingMethod, sourceFrame.GetFileLineNumber());
        }

        /// <summary>
        /// Log the Exception
        /// </summary>
        /// <param name="ex">The Exception</param>
        private static void LogException(Exception ex)
        {
            if (ex == null) return;

            Debug.WriteLine("Exception handled on main host thread", ex);
            

        }

        /// <summary>
        /// Initialize DependencyFactory
        /// </summary>
        private static void InitializeDependencyFactory()
        {
            DependencyFactory.InterceptionBehaviourFactory = new ServerInterceptionBehaviourFactory();

            // cache manager            
            DependencyFactory.RegisterInstanceIfMissing(() => CacheFactory.GetCacheManager(CacheIdentifiers.TodolistBussineServiceCache), CacheIdentifiers.TodolistBussineServiceCache);

        }

        /// <summary>
        /// Initialize Modules
        /// </summary>
        private static void InitializeAndStartModules()
        {
            var sw = new Stopwatch();
            sw.Start();

            string path = Directory.GetCurrentDirectory();

            var assemblyLocation = AppDomain.CurrentDomain.BaseDirectory + "bin";

            Assembly asm = Assembly.GetExecutingAssembly();
            _log.TraceFormat("\r\n\nServer path : {0} current {1}", asm.CodeBase, path);
            _log.DebugFormat("Searching for assemblies in path '{0}'", assemblyLocation);

            //Print the file name and version number. is in Autoconfiguredservicehost.cs

            var dlls = Directory.GetFiles(assemblyLocation, "Contoso.Serv*.*.dll");
            var assemblies = dlls.Select(Assembly.LoadFrom).ToList();
            _log.DebugFormat("Found {0} assemblies: {1}", assemblies.Count, string.Join(", ", assemblies));

            var dynamicModules = assemblies.SelectMany(AllDynamicModules).ToList();
            var startableDynamicModules = dynamicModules.OfType<IStartableDynamicModule>().ToList();

            _log.DebugFormat("Found {0} IDynamicModules: {1}", dynamicModules.Count(), string.Join(", ", dynamicModules));

            using (new ProfilingScope(string.Format("Initialized {0} IDynamicModules ", dynamicModules.Count())))
            {
                foreach (var module in dynamicModules)
                {
                    using (new ProfilingScope(string.Format("Initialized Module '{0}' ", module.GetType().Name)))
                    {
                        module.Initialize();
                    }
                }
            }

            using (new ProfilingScope(string.Format("Started {0} IStartableDynamicModules ", startableDynamicModules.Count())))
                try
                {
                    startableDynamicModules.ForEach(module => module.Start());
                }
                catch (Exception ex)
                {
                    _log.Error("Error Starting Module", ex);
                    throw;
                }
            // Display information about each assembly loaded into this AppDomain.

            foreach (Assembly myassembly in assemblies)
            {
                _log.InfoFormat("Assembly  {0}", myassembly);
                foreach (AssemblyName an in myassembly.GetReferencedAssemblies())
                {
                    _log.InfoFormat("reference Name={0}, Version={1}, Culture={2}, PublicKey token={3} , Path {4}", an.Name, an.Version, an.CultureInfo.Name, (BitConverter.ToString(an.GetPublicKeyToken())), an.CodeBase);
                }
            }

            InitializeDependencyFactory();

            sw.Stop();
        
            ILog serviceTimeLog = LogManager.GetLogger(LogCategories.Infrastructure);

#if DEBUG
                serviceTimeLog.InfoFormat("Contoso Server Started in Debug Mode");
                _log.DebugFormat("Contoso Server Start in Debug Mode took  {0} ", sw.Elapsed);

#else
            serviceTimeLog.InfoFormat("Contoso Server Started in release Mode");
            _log.DebugFormat("Contoso Server Start in release Mode took  {0} ", sw.Elapsed);
#endif

        }
           
    }
}