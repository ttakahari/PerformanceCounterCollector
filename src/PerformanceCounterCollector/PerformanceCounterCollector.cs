using System;
using System.Diagnostics;
using System.Linq;

namespace PerformanceCounterCollector
{
    /// <summary>
    /// The class that collects values of Performance Counter.
    /// </summary>
    public class PerformanceCounterCollector : IDisposable
    {
        private bool _disposed;

        private readonly PerformanceCounter[] _counters;
        private readonly Action<PerformanceCounter, float> _valueHandler;
        private readonly Action<PerformanceCounter, Exception> _exceptionHandler;

        /// <summary>
        /// Create a new <see cref="PerformanceCounterCollector"/> instance.
        /// </summary>
        /// <param name="counter">The instance of the target of Performance Counter.</param>
        /// <param name="valueHandler">How to handle a counter value.</param>
        /// <param name="exceptionHandler">How to handle exceptions that occur when getting a counter value.</param>
        public PerformanceCounterCollector(
            PerformanceCounter counter,
            Action<PerformanceCounter, float> valueHandler,
            Action<PerformanceCounter, Exception> exceptionHandler = null
            ) : this(new[] { counter }, valueHandler, exceptionHandler)
        { }

        /// <summary>
        /// Create a new <see cref="PerformanceCounterCollector"/> instance.
        /// </summary>
        /// <param name="counters">The instances of the targets of Performance Counter.</param>
        /// <param name="valueHandler">How to handle a counter value.</param>
        /// <param name="exceptionHandler">How to handle exceptions that occur when getting a counter value.</param>
        public PerformanceCounterCollector(
            PerformanceCounter[] counters,
            Action<PerformanceCounter, float> valueHandler,
            Action<PerformanceCounter, Exception> exceptionHandler = null)
        {
            if (counters     == null) throw new ArgumentNullException(nameof(counters));
            if (valueHandler == null) throw new ArgumentNullException(nameof(valueHandler));

            if (!counters.Any() || counters.All(x => x == null))
            {
                throw new ArgumentOutOfRangeException(nameof(counters), $"{typeof(PerformanceCounter).FullName} instance is required at least one or more.");
            }

            _counters         = counters;
            _valueHandler     = valueHandler;
            _exceptionHandler = exceptionHandler;
        }

        /// <summary>
        /// Collect values of Performance Counter.
        /// </summary>
        public void Collect()
        {
            foreach (var counter in _counters)
            {
                try
                {
                    var value = counter.NextValue();

                    _valueHandler.Invoke(counter, value);
                }
                catch (Exception ex)
                {
                    _exceptionHandler?.Invoke(counter, ex);
                }
            }
        }

        ~PerformanceCounterCollector()
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
                foreach (var counter in _counters)
                {
                    counter.Dispose();
                }
            }

            _disposed = true;
        }
    }
}