using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace PerformanceCounterCollector.Tests
{
    public class PerformanceCounterCollectorTests
    {
        private readonly ITestOutputHelper _helper;

        public PerformanceCounterCollectorTests(ITestOutputHelper helper)
        {
            _helper = helper;
        }

        [Fact]
        public void Constructor_Tests()
        {
            var performanceCounter  = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            var performanceCounters = new[]
            {
                new PerformanceCounter("Processor"   , "% Processor Time", "_Total"),
                new PerformanceCounter("PhysicalDisk", "% Disk Time"     , "_Total")
            };
            var valueHandler        = new Action<PerformanceCounter, float>((counter, value) => { });
            var exceptionHandler    = new Action<PerformanceCounter, Exception>((counter, ex) => { });

            // constructor by single counter
            {
                var constructor = GetConstructor<PerformanceCounter>();
                Assert.Throws<ArgumentOutOfRangeException>(() => constructor.Invoke(null, valueHandler, null));

                Assert.Throws<ArgumentNullException>(() => new PerformanceCounterCollector(performanceCounter, null));

                using (var collector = new PerformanceCounterCollector(performanceCounter, valueHandler))
                {
                    Assert.NotNull(collector);
                }

                using (var collector = new PerformanceCounterCollector(performanceCounter, valueHandler, null))
                {
                    Assert.NotNull(collector);
                }

                using (var collector = new PerformanceCounterCollector(performanceCounter, valueHandler, exceptionHandler))
                {
                    Assert.NotNull(collector);
                }
            }

            // constructor by multi counters
            {
                var constructor = GetConstructor<PerformanceCounter[]>();
                Assert.Throws<ArgumentNullException>(() => constructor.Invoke(null, valueHandler, exceptionHandler));

                Assert.Throws<ArgumentNullException>(() => new PerformanceCounterCollector(performanceCounters, null));
                Assert.Throws<ArgumentOutOfRangeException>(() => new PerformanceCounterCollector(Array.Empty<PerformanceCounter>(), valueHandler));
                Assert.Throws<ArgumentOutOfRangeException>(() => new PerformanceCounterCollector(new PerformanceCounter[] { null }, valueHandler));

                using (var collector = new PerformanceCounterCollector(performanceCounters, valueHandler))
                {
                    Assert.NotNull(collector);
                }

                using (var collector = new PerformanceCounterCollector(performanceCounters, valueHandler, null))
                {
                    Assert.NotNull(collector);
                }

                using (var collector = new PerformanceCounterCollector(performanceCounters, valueHandler, exceptionHandler))
                {
                    Assert.NotNull(collector);
                }
            }
        }

        [Fact]
        public void Collect_Tests()
        {
            // single counter
            {
                // regular
                using (var collector = new PerformanceCounterCollector(
                    new PerformanceCounter("Processor", "% Processor Time", "_Total"),
                    (counter, value) => Assert.True(-1 < value),
                    (counter, exception) => Assert.False(true)))
                {
                    for (var i = 0; i < 5; i++)
                    {
                        collector.Collect();

                        Task.Delay(100).Wait();
                    }
                }

                // error
                using (var collector = new PerformanceCounterCollector(
                    new PerformanceCounter("Processor", "% Processor Time"),
                    (counter, value) => Assert.False(true),
                    (counter, exception) => Assert.NotNull(exception)))
                {
                    for (var i = 0; i < 5; i++)
                    {
                        collector.Collect();

                        Task.Delay(100).Wait();
                    }
                }
            }

            // multi counter
            {
                // success
                using (var collector = new PerformanceCounterCollector(
                    new[]
                    {
                        new PerformanceCounter("Processor"   , "% Processor Time", "_Total"),
                        new PerformanceCounter("PhysicalDisk", "% Disk Time"     , "_Total")
                    },
                    (counter, value) =>
                    {
                        _helper.WriteLine($"{counter.CategoryName} / {counter.CounterName} / {counter.InstanceName ?? "--"} : {value}");

                        Assert.True(counter.CategoryName == "Processor" || counter.CategoryName == "PhysicalDisk");
                        Assert.True(-1 < value);
                    },
                    (counter, exception) => Assert.False(true)))
                {
                    for (var i = 0; i < 5; i++)
                    {
                        collector.Collect();

                        Task.Delay(100).Wait();
                    }
                }

                // failure
                using (var collector = new PerformanceCounterCollector(
                    new[]
                    {
                        new PerformanceCounter("Processor"   , "% Processor Time"),
                        new PerformanceCounter("PhysicalDisk", "% Disk Time")
                    },
                    (counter, value) => Assert.False(true),
                    (counter, exception) =>
                    {
                        _helper.WriteLine($"{counter.CategoryName} / {counter.CounterName} / {counter.InstanceName ?? "--"} : {exception}");

                        Assert.True(counter.CategoryName == "Processor" || counter.CategoryName == "PhysicalDisk");
                        Assert.NotNull(exception);
                    }))
                {
                    for (var i = 0; i < 5; i++)
                    {
                        collector.Collect();

                        Task.Delay(100).Wait();
                    }
                }

                // success and failure
                using (var collector = new PerformanceCounterCollector(
                    new[]
                    {
                        new PerformanceCounter("Processor"   , "% Processor Time"),
                        new PerformanceCounter("PhysicalDisk", "% Disk Time", "_Total")
                    },
                    (counter, value) =>
                    {
                        _helper.WriteLine($"{counter.CategoryName} / {counter.CounterName} / {counter.InstanceName ?? "--"} : {value}");

                        Assert.True(counter.CategoryName == "PhysicalDisk");
                        Assert.True(-1 < value);
                    },
                    (counter, exception) =>
                    {
                        _helper.WriteLine($"{counter.CategoryName} / {counter.CounterName} / {counter.InstanceName ?? "--"} : {exception}");

                        Assert.True(counter.CategoryName == "Processor");
                        Assert.NotNull(exception);
                    }))
                {
                    for (var i = 0; i < 5; i++)
                    {
                        collector.Collect();

                        Task.Delay(100).Wait();
                    }
                }
            }
        }

        [Fact]
        public void Dispose_Tests()
        {
            var collector = new PerformanceCounterCollector(
                new PerformanceCounter("Processor", "% Processor Time", "_Total"),
                (counter, value) => { },
                (counter, exception) => { });

            collector.Dispose();
            collector.Dispose();
        }

        private static Func<T, Action<PerformanceCounter, float>, Action<PerformanceCounter, Exception>, PerformanceCounterCollector> GetConstructor<T>()
        {
            var constructorInfo = typeof(PerformanceCounterCollector).GetConstructors()
                .First(x =>
                {
                    var parameters = x.GetParameters();

                    return parameters[0].ParameterType == typeof(T)
                           && parameters[2].GetCustomAttribute(typeof(OptionalAttribute), false) != null;
                });

            var arg1 = Expression.Parameter(typeof(T), typeof(T) is PerformanceCounter ? "counter" : "counters");
            var arg2 = Expression.Parameter(typeof(Action<PerformanceCounter, float>), "valueHandler");
            var arg3 = Expression.Parameter(typeof(Action<PerformanceCounter, Exception>), "exceptionHandler");

            var constructor = Expression.Lambda<Func<T, Action<PerformanceCounter, float>, Action<PerformanceCounter, Exception>, PerformanceCounterCollector>>(
                Expression.New(constructorInfo, arg1, arg2, arg3),
                arg1, arg2, arg3).Compile();

            return constructor;
        }
    }
}