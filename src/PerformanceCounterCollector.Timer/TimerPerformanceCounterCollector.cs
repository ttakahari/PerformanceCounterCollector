using System;

namespace PerformanceCounterCollector.Timer
{
    /// <summary>
    /// The class that collects values of Performance Counter with using <see cref="System.Threading.Timer"/>.
    /// </summary>
    public class TimerPerformanceCounterCollector : IDisposable
    {
        private bool _disposed;
        private System.Threading.Timer _timer;

        private readonly PerformanceCounterCollector _collector;
        private readonly TimeSpan _interval;

        /// <summary>
        /// Create a new <see cref="TimerPerformanceCounterCollector"/> instance.
        /// </summary>
        /// <param name="collector">The instance of <see cref="PerformanceCounterCollector"/>.</param>
        /// <param name="interval">The execution interval.</param>
        public TimerPerformanceCounterCollector(PerformanceCounterCollector collector, TimeSpan interval)
        {
            if (collector == null)          throw new ArgumentNullException(nameof(collector));
            if (interval  <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(interval), $"The interval must be greater than {TimeSpan.Zero}.");

            _collector = collector;
            _interval  = interval;
        }

        /// <summary>
        /// Collect values of Performance Counter.
        /// </summary>
        public void Collect()
        {
            _timer?.Dispose();

            _timer = new System.Threading.Timer(
                _ => _collector.Collect(),
                null,
                _interval,
                _interval);
        }

        ~TimerPerformanceCounterCollector()
        {
            Dispose(false);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _timer?.Dispose();

                _collector.Dispose();
            }

            _disposed = true;
        }
    }
}