using System;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace PerformanceCounterCollector.Tests
{
    public class PerformanceCounterFactoryTests
    {
        private readonly ITestOutputHelper _helper;

        public PerformanceCounterFactoryTests(ITestOutputHelper helper)
        {
            _helper = helper;
        }

        [Fact]
        public void Create_Tests()
        {
            // by category name & counter name.
            {
                Assert.Throws<ArgumentNullException>(() => PerformanceCounterFactory.Create("", "% Processor Time"));
                Assert.Throws<ArgumentNullException>(() => PerformanceCounterFactory.Create("Processor", ""));

                var counter = PerformanceCounterFactory.Create("Processor", "% Processor Time");
                Assert.NotNull(counter);
            }

            // by category name, counter name & instance name.
            {
                Assert.Throws<ArgumentNullException>(() => PerformanceCounterFactory.Create("", "% Processor Time", "_Total"));
                Assert.Throws<ArgumentNullException>(() => PerformanceCounterFactory.Create("Processor", "", "_Total"));
                Assert.Throws<ArgumentNullException>(() => PerformanceCounterFactory.Create("Processor", "% Processor Time", ""));

                var counter = PerformanceCounterFactory.Create("Processor", "% Processor Time", "_Total");
                Assert.NotNull(counter);
            }

            // by category name.
            {
                Assert.Throws<ArgumentNullException>(() => PerformanceCounterFactory.Create(""));
                Assert.Throws<InvalidOperationException>(() => PerformanceCounterFactory.Create("Foo")); // invalid category name.

                {
                    var counters = PerformanceCounterFactory.Create("Processor");
                    Assert.NotNull(counters);
                    Assert.True(counters.Any());
                }

                {
                    var counters = PerformanceCounterFactory.Create("Memory");
                    Assert.NotNull(counters);
                    Assert.True(counters.Any());
                }
            }
        }
    }
}