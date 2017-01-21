using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace PerformanceCounterCollector.Rx.Tests
{
    public class ReactivePerformanceCounterCollectorTests
    {
        private readonly ITestOutputHelper _helper;

        public ReactivePerformanceCounterCollectorTests(ITestOutputHelper helper)
        {
            _helper = helper;
        }

        [Fact]
        public void Constructor_Tests()
        {
            var collector = new PerformanceCounterCollector(
                new PerformanceCounter("Processor", "% Processor Time", "_Total"),
                (counter, value) => { },
                (counter, exception) => { });
            var interval  = TimeSpan.FromMilliseconds(100);

            Assert.Throws<ArgumentNullException>(() => new ReactivePerformanceCounterCollector(null, interval));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ReactivePerformanceCounterCollector(collector, TimeSpan.Zero));

            using (var reactiveCollector =  new ReactivePerformanceCounterCollector(collector, interval))
            {
                Assert.NotNull(reactiveCollector);
            }
        }

        [Fact]
        public void Collect_Tests()
        {
            // success
            using (var collector = new ReactivePerformanceCounterCollector(
                new PerformanceCounterCollector(
                    new[]
                    {
                        new PerformanceCounter("Processor", "% Processor Time", "_Total"),
                        new PerformanceCounter("PhysicalDisk", "% Disk Time", "_Total")
                    },
                    (counter, value) =>
                    {
                        _helper.WriteLine($"{counter.CategoryName} / {counter.CounterName} / {counter.InstanceName ?? "--"} : {value}");

                        Assert.True(counter.CategoryName == "Processor" || counter.CategoryName == "PhysicalDisk");
                        Assert.True(-1 < value);
                    },
                    (counter, exception) => Assert.False(true)
                ),
                TimeSpan.FromMilliseconds(100)))
            {
                collector.Collect();

                Task.Delay(1000).Wait();
            }

            // failure
            using (var collector = new ReactivePerformanceCounterCollector(
                new PerformanceCounterCollector(
                    new[]
                    {
                        new PerformanceCounter("Processor", "% Processor Time"),
                        new PerformanceCounter("PhysicalDisk", "% Disk Time")
                    },
                    (counter, value) => Assert.True(-1 < value),
                    (counter, exception) =>
                    {
                        _helper.WriteLine($"{counter.CategoryName} / {counter.CounterName} / {counter.InstanceName ?? "--"} : {exception}");

                        Assert.True(counter.CategoryName == "Processor" || counter.CategoryName == "PhysicalDisk");
                        Assert.NotNull(exception);
                    }
                ),
                TimeSpan.FromMilliseconds(100)))
            {
                collector.Collect();

                Task.Delay(1000).Wait();
            }

            // success and failure
            using (var collector = new ReactivePerformanceCounterCollector(
                new PerformanceCounterCollector(
                    new[]
                    {
                        new PerformanceCounter("Processor", "% Processor Time"),
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
                    }
                ),
                TimeSpan.FromMilliseconds(100)))
            {
                collector.Collect();

                Task.Delay(1000).Wait();
            }
        }

        [Fact]
        public void Dispose_Tests()
        {
            var collector = new PerformanceCounterCollector(
                new PerformanceCounter("Processor", "% Processor Time", "_Total"),
                (counter, value) => { },
                (counter, exception) => { });
            var interval  = TimeSpan.FromMilliseconds(100);

            var reactiveCollector = new ReactivePerformanceCounterCollector(collector, interval);

            reactiveCollector.Dispose();
            reactiveCollector.Dispose();
        }
    }
}