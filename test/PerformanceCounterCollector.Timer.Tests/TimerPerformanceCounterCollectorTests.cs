using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace PerformanceCounterCollector.Timer.Tests
{
    public class TimerPerformanceCounterCollectorTests
    {
        private readonly ITestOutputHelper _helper;

        public TimerPerformanceCounterCollectorTests(ITestOutputHelper helper)
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

            Assert.Throws<ArgumentNullException>(() => new TimerPerformanceCounterCollector(null, interval));
            Assert.Throws<ArgumentOutOfRangeException>(() => new TimerPerformanceCounterCollector(collector, TimeSpan.Zero));

            using (var timerCollector =  new TimerPerformanceCounterCollector(collector, interval))
            {
                Assert.NotNull(timerCollector);
            }
        }

        [Fact]
        public void Collect_Tests()
        {
            // success
            using (var collector = new TimerPerformanceCounterCollector(
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
            using (var collector = new TimerPerformanceCounterCollector(
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
            using (var collector = new TimerPerformanceCounterCollector(
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

            var timerCollector = new TimerPerformanceCounterCollector(collector, interval);

            timerCollector.Dispose();
            timerCollector.Dispose();
        }
    }
}