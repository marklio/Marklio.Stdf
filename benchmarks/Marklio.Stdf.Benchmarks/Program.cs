using BenchmarkDotNet.Running;
using Marklio.Stdf.Benchmarks;

BenchmarkSwitcher.FromAssembly(typeof(ReadBenchmarks).Assembly).Run(args);
