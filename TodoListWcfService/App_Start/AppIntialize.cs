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
        //private static ILog _log = LogManager.GetLogger(LogCategories.Infrastructure);

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

            InitializeDependencyFactory();

#if DEBUG
                Debug.WriteLine(string.Format(" Server Started in Debug Mode took  {0} ", sw.Elapsed));
  
#else
            Debug.WriteLine(string.Format(" Server Start in release Mode took  {0} ", sw.Elapsed));
#endif

        }
    }
}