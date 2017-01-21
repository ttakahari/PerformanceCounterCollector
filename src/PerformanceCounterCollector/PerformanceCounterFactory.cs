using System;
using System.Diagnostics;
using System.Linq;

namespace PerformanceCounterCollector
{
    /// <summary>
    /// The class that creates a new <see cref="PerformanceCounter"/> instance or instances.
    /// </summary>
    public class PerformanceCounterFactory
    {
        /// <summary>
        /// Create a new <see cref="PerformanceCounter"/> instance by the category name and the counter name.
        /// </summary>
        /// <param name="categoryName">The category name.</param>
        /// <param name="counterName">The counter name.</param>
        /// <returns>The created <see cref="PerformanceCounter"/> instance.</returns>
        public static PerformanceCounter Create(string categoryName, string counterName)
        {
            if (string.IsNullOrEmpty(categoryName)) throw new ArgumentNullException(nameof(categoryName));
            if (string.IsNullOrEmpty(counterName))  throw new ArgumentNullException(nameof(counterName));

            return new PerformanceCounter(categoryName, counterName);
        }

        /// <summary>
        /// Create a new <see cref="PerformanceCounter"/> instance by the category name, the counter name and the instance name.
        /// </summary>
        /// <param name="categoryName">The category name.</param>
        /// <param name="counterName">The counter name.</param>
        /// <param name="instanceName">The instance name.</param>
        /// <returns>The created <see cref="PerformanceCounter"/> instance.</returns>
        public static PerformanceCounter Create(string categoryName, string counterName, string instanceName)
        {
            if (string.IsNullOrEmpty(categoryName)) throw new ArgumentNullException(nameof(categoryName));
            if (string.IsNullOrEmpty(counterName))  throw new ArgumentNullException(nameof(counterName));
            if (string.IsNullOrEmpty(instanceName)) throw new ArgumentNullException(nameof(instanceName));

            return new PerformanceCounter(categoryName, counterName, instanceName);
        }

        /// <summary>
        /// Create new <see cref="PerformanceCounter"/> instances by the category name.
        /// </summary>
        /// <param name="categoryName">The category name.</param>
        /// <returns>The created <see cref="PerformanceCounter"/> instances.</returns>
        public static PerformanceCounter[] Create(string categoryName)
        {
            if (string.IsNullOrEmpty(categoryName))
            {
                throw new ArgumentNullException(nameof(categoryName));
            }

            var category = new PerformanceCounterCategory(categoryName);

            var instanceNames = category.GetInstanceNames();

            return instanceNames.Any()
                ? instanceNames
                    .SelectMany(x => category.GetCounters(x))
                    .ToArray()
                : category.GetCounters();
        }
    }
}