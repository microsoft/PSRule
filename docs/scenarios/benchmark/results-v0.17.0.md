``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.18363.778 (1909/November2018Update/19H2)
Intel Core i7-6600U CPU 2.60GHz (Skylake), 1 CPU, 4 logical and 2 physical cores
.NET Core SDK=3.1.201
  [Host]     : .NET Core 2.1.17 (CoreCLR 4.6.28619.01, CoreFX 4.6.28619.01), X64 RyuJIT
  DefaultJob : .NET Core 2.1.17 (CoreCLR 4.6.28619.01, CoreFX 4.6.28619.01), X64 RyuJIT


```
|                   Method |       Mean |     Error |    StdDev |     Median |      Gen 0 | Gen 1 | Gen 2 |   Allocated |
|------------------------- |-----------:|----------:|----------:|-----------:|-----------:|------:|------:|------------:|
|                   Invoke | 111.140 ms | 2.1935 ms | 4.5786 ms | 109.312 ms |  8200.0000 |     - |     - | 16839.42 KB |
|                 InvokeIf | 117.141 ms | 2.2703 ms | 2.2298 ms | 116.398 ms |  9600.0000 |     - |     - | 19980.62 KB |
|               InvokeType | 108.648 ms | 0.7983 ms | 0.7467 ms | 108.584 ms |  8200.0000 |     - |     - | 16870.67 KB |
|            InvokeSummary | 107.300 ms | 0.8612 ms | 0.8056 ms | 107.115 ms |  8000.0000 |     - |     - | 16784.76 KB |
|                      Get |   9.003 ms | 0.0643 ms | 0.0602 ms |   9.010 ms |   140.6250 |     - |     - |   307.96 KB |
|                  GetHelp |   8.902 ms | 0.0831 ms | 0.0649 ms |   8.899 ms |   140.6250 |     - |     - |   306.34 KB |
|                   Within | 179.522 ms | 1.5483 ms | 1.4483 ms | 179.981 ms | 15666.6667 |     - |     - | 32400.38 KB |
|               WithinBulk | 247.883 ms | 2.6279 ms | 2.1944 ms | 248.124 ms | 28500.0000 |     - |     - | 59306.73 KB |
|               WithinLike | 238.815 ms | 2.5538 ms | 1.9939 ms | 239.245 ms | 29333.3333 |     - |     - | 60580.58 KB |
| DefaultTargetNameBinding |   2.124 ms | 0.0214 ms | 0.0200 ms |   2.129 ms |    85.9375 |     - |     - |   179.69 KB |
|  CustomTargetNameBinding |   2.463 ms | 0.0483 ms | 0.0452 ms |   2.458 ms |   179.6875 |     - |     - |      375 KB |
|  NestedTargetNameBinding |   2.433 ms | 0.0370 ms | 0.0328 ms |   2.420 ms |   179.6875 |     - |     - |      375 KB |
