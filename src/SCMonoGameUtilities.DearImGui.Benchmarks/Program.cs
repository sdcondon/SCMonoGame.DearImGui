using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using System.Reflection;

// Run this in "Release" and without a debugger attached.
// See https://benchmarkdotnet.org/articles/guides/console-args.html (or run app with --help)

BenchmarkSwitcher.FromAssembly(Assembly.GetExecutingAssembly()).Run(args, GetGlobalConfig());

static IConfig GetGlobalConfig()
{
    return DefaultConfig.Instance
        .WithOptions(ConfigOptions.DisableOptimizationsValidator);
}