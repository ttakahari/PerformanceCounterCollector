using System;
using System.Reactive.Linq;

namespace PerformanceCounterCollector.Rx
{
    /// <summary>
    /// The class that collects values of Performance Counter with using Reactive-Extensions.
    /// </summary>
    public class ReactivePerformanceCounterCollector : IDisposable
    {
        private bool _disposed;
        private IDisposable _observable;

        private readonly PerformanceCounterCollector _collector;
        private readonly TimeSpan _interval;

        /// <summary>
        /// Create a new <see cref="ReactivePerformanceCounterCollector"/> instance.
        /// </summary>
        /// <param name="collector">The instance of <see cref="PerformanceCounterCollector"/>.</param>
        /// <param name="interval">The execution interval.</param>
        public ReactivePerformanceCounterCollector(PerformanceCounterCollector collector, TimeSpan interval)
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
            _observable?.Dispose();

            _observable = Observable.Interval(_interval)
                .Subscribe(_ => _collector.Collect());
        }

        ~ReactivePerformanceCounterCollector()
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
                _observable?.Dispose();

                _collector.Dispose();
            }

            _disposed = true;
        }
    }
}