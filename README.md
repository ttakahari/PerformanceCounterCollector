# PerformanceCounterCollector

The library for collecting values of Performance Counter, and for helping it.

If you want to collect at regular intervals, jump to [here](https://github.com/ttakahari/PerformanceCounterCollector#intervals).

## Install

from NuGet - [PerformanceCounterCollector](https://www.nuget.org/packages/PerformanceCounterCollector/)

```ps1
PM > Install-Package PerformanceCounterCollector
```

## How to use

At first, you must create a instance or instances of ```PerformanceCounter```.

Next, you create a instance of ```PerformanceCounterCollector```, with giving the counter instance and ```delegate``` how to handle the value of Performance Counter.  
(You can add how to handle exceptions that occur when getting the value of Performance Counter, as the optional argument.)

At last, you call ```Collect``` method.

```csharp
// create instance of PerformanceCounter
var counter = new PerformanceCounter("Processor", "% Processor Time", "_Total");

// create instance of PerformanceCounterCollector
var collector = new PerformanceCounterCollector(
    counter,
    (target, value) => Trace.TraceInformation($"{target.CategoryName} / {target.CounterName} / {target.InstanceName} - {value}"),
    (target, exception) => Trace.TraceError($"{target.CategoryName} / {target.CounterName} / {target.InstanceName} - {value}")
);

// collect values of PerformanceCounter
collector.Collect();
```

## Utility

```PerformanceCounterFactory``` class helps you to create a instance or instances of ```PerformanceCounter```.

Especially, it is useful when you don't know instance names of Performance Counter because they are dynamic.

```csharp
// If you know a counter name but don't know instance names, you can take all counters with giving tha category name of Performance Counter.
var counters = PerformanceCounterFactory.Create("Processor");
```

## Intervals

If you want to collect values of Performance Counter at regular intervals, you should use PerformanceCounterCollector.Rx(using Reactive-Extensions.) or PerformanceCounterCollector.Timer(using ```System.Threading.Timer```).

Both are available from NuGet, also.

* [PerformanceCounterCollector.Rx](https://www.nuget.org/packages/PerformanceCounterCollector.Rx/) (To collect values of Performance Counter at regular intervals with using Reactive-Extensions.)

```ps1
PM > Install Package PerformanceCounterCollector.Rx
```

* [PerformanceCounterCollector.Timer](https://www.nuget.org/packages/PerformanceCounterCollector.Timer/)

```ps1
PM > Install Package PerformanceCounterCollector.Timer
```

The difference is how to take intervals.

Reactive-Extensions takes intervals after a process finishes, and ```System.Threading.Timer``` takes intervals immediately after a process begins.

![difference](https://github.com/ttakahari/PerformanceCounterCollector/blob/master/doc/difference.png)

You can keep collecting values of Performance Counter at regular intervals if you create a instance of ```ReactivePerformanceCounterCollector``` or ```TimerPerformanceCounterCollector``` and call ```Collect``` method, while the instance keeping alive.

```charp
// At first create a instance of PerformanceCounterCollector
var innerCollector = new PerformanceCounterCollector(
    counter,
    (target, value) => Trace.TraceInformation($"{target.CategoryName} / {target.CounterName} / {target.InstanceName} - {value}"),
    (target, exception) => Trace.TraceError($"{target.CategoryName} / {target.CounterName} / {target.InstanceName} - {value}")
);

// Next, create a instance of ReactivePerformanceCounterCollector or TimerPerformanceCounterCollector,
// giving the instance of PerformanceCounterCollector and interval span(System.TimeSpan).
var collector = new ReactivePerformanceCounterCollector(innterCollector, TimeSpan.FromSeconds(60));

// Finally, call Collect method. (If you want to keep collecting, this instance should be static instance.)
collector.Collect();
```

## Lisence

under [MIT Lisence](https://opensource.org/licenses/MIT).
