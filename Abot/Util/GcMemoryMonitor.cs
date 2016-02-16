using log4net;
using System;
using System.Diagnostics;

namespace Abot.Util
{
    public interface IMemoryMonitor : IDisposable
    {
        int GetCurrentUsageInMb();
    }

    [Serializable]
    public class GcMemoryMonitor : IMemoryMonitor
    {
        static readonly ILog _logger = LogManager.GetLogger(typeof(GcMemoryMonitor));

        public virtual int GetCurrentUsageInMb()
        {
            Stopwatch timer = Stopwatch.StartNew();
            int currentUsageInMb = Convert.ToInt32(GC.GetTotalMemory(false) / (1024 * 1024));
            timer.Stop();

            _logger.DebugFormat("GC reporting [{0}mb] currently thought to be allocated, took [{1}] millisecs", currentUsageInMb, timer.ElapsedMilliseconds);

            return currentUsageInMb;       
        }

        public void Dispose()
        {
            //do nothing
        }
    }
}
