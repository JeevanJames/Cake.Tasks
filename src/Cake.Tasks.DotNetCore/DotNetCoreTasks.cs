using System;
using System.IO;
using Cake.Common.Tools.DotNetCore;
using Cake.Common.Tools.DotNetCore.Build;
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
            if (!config.TryResolve("DOTNETCORE_SolutionFiles", out object solutionFiles))
            {
                context.Log.Warning("No solution files found to build.");
                return;
            }

            if (solutionFiles is string slnFile)
            {
                context.DotNetCoreBuild(slnFile);
            }
        }

        [CoreTask(CoreTask.Test)]
        public static void Test(this ICakeContext context, TaskConfig config)
        {
            if (!config.TryResolve("DOTNETCORE_SolutionFiles", out object solutionFiles))
            {
                context.Log.Warning("No solution files found to test.");
                return;
            }

            if (solutionFiles is string slnFile)
            {
                context.DotNetCoreTest(slnFile);
            }
        }

        [Config]
        public static void Config(this ICakeContext context, TaskConfig config)
        {
            config.Register("DOTNETCORE_SolutionFiles", (Func<object>)(() =>
            {
                string workingDirectory = config.Resolve("ENV_WorkingDirectory", Directory.GetCurrentDirectory());
                string[] slnFiles = Directory.GetFiles(workingDirectory, "*.sln", SearchOption.TopDirectoryOnly);
                if (slnFiles.Length == 0)
                    slnFiles = Directory.GetFiles(workingDirectory, "*.sln", SearchOption.AllDirectories);
                if (slnFiles.Length > 1)
                    return slnFiles;
                if (slnFiles.Length == 1)
                    return slnFiles[0];
                return null;
            }));
        }
    }
}
