﻿using System;

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
            if (interval <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(interval), $"The interval must be greater than {TimeSpan.Zero}.");

            _collector = collector ?? throw new ArgumentNullException(nameof(collector));
            _interval = interval;
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

        /// <summary>
        /// Destructor for not calling <see cref="Dispose()"/> method.
        /// </summary>
        ~TimerPerformanceCounterCollector()
            => Dispose(false);

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Free, release, or reset managed or unmanaged resources.
        /// </summary>
        /// <param name="disposing">Wether to free, release, or resetting unmanaged resources or not.</param>
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