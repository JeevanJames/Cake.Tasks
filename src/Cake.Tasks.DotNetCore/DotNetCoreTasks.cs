using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Tasks.Core;
using Cake.Tasks.DotNetCore;

[assembly: TaskPlugin(typeof(DotNetCoreTasks))]

namespace Cake.Tasks.DotNetCore
{
    public static class DotNetCoreTasks
    {
        [CoreTask(CoreTask.Build)]
        public static void Build(this ICakeContext context, TaskConfig config)
        {
            string solutionFile = config.Resolve<string>("SolutionFile", null);
            if (solutionFile is null)
                context.Log.Information($".NET Core build task (No solution file)");
            else
                context.Log.Information($".NET Core build task ({solutionFile})");
        }

        [Config]
        public static void Config(this ICakeContext context, TaskConfig config)
        {
            config.Data.Add("SolutionFile", "MySolution.sln");
        }
    }
}
