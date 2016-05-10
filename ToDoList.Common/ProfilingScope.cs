using System;
using System.Diagnostics;

namespace ToDoList.Common
{
    /// <summary>
    /// Scope for Profiling Logging Only when DEBUG Mode
    /// </summary>
    public sealed class ProfilingScope : IDisposable
    {
        /// <summary>
        /// Profiling Logger
        /// </summary>
       // private static readonly ILog Log = LogManager.GetLogger(LogCategories.Profiling);
        private readonly Stopwatch stopWatch;
        private readonly string operationName;

        private readonly bool isProfilingEnabled = true;//Log.IsDebugEnabled;

        public ProfilingScope(string operationName)
        {
#if DEBUG
            isProfilingEnabled = true;
#endif
            if (this.isProfilingEnabled)
            {
                stopWatch = new Stopwatch();
                stopWatch.Start();
                this.operationName = operationName;
            }
        }

        public TimeSpan Elapsed
        {
            get
            {
                return stopWatch.Elapsed;
            }
        }

        public void Dispose()
        {
            if (this.isProfilingEnabled)
            {
                stopWatch.Stop();
                //Log.DebugFormat("Operation {0} took {1}", operationName, stopWatch.Elapsed);
                Debug.WriteLine("Operation {0} took {1}", operationName, stopWatch.Elapsed);
            }
        }
    }
}
