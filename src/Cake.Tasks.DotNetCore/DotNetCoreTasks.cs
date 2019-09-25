using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Cake.Common.Tools.DotNetCore;
using Cake.Common.Tools.DotNetCore.Build;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Tasks.Config;
using Cake.Tasks.Core;
using Cake.Tasks.DotNetCore;

[assembly: TaskPlugin(typeof(DotNetCoreTasks))]

namespace Cake.Tasks.DotNetCore
{
    public static class DotNetCoreTasks
    {
        [TaskEvent(TaskEventType.BeforeTask, CoreTask.Build)]
        public static void Clean(ICakeContext context, TaskConfig config)
        {
            var cfg = config.Load<DotNetCoreConfig>();
            IEnumerable<string> cleanProjectFiles = cfg.BuildProjectFiles.Resolve();
            if (cleanProjectFiles is null || !cleanProjectFiles.Any())
            {
                context.Log.Warning("No solution or project files found to clean.");
                return;
            }

            foreach (string cleanProjectFile in cleanProjectFiles)
                context.DotNetCoreClean(cleanProjectFile);
        }

        [CoreTask(CoreTask.Build)]
        public static void Build(this ICakeContext context, TaskConfig config)
        {
            var cfg = config.Load<DotNetCoreConfig>();
            var env = config.Load<EnvConfig>();

            IEnumerable<string> buildProjectFiles = cfg.BuildProjectFiles.Resolve();
            if (buildProjectFiles is null || !buildProjectFiles.Any())
            {
                context.Log.Warning("No solution or project files found to build.");
                return;
            }

            foreach (string buildProjectFile in buildProjectFiles)
            {
                context.DotNetCoreBuild(buildProjectFile, new DotNetCoreBuildSettings
                {
                    Configuration = env.Configuration,
                });
            }
        }

        [CoreTask(CoreTask.Test)]
        public static void Test(this ICakeContext context, TaskConfig config)
        {
            var cfg = config.Load<DotNetCoreConfig>();
            IEnumerable<string> testProjectFiles = cfg.TestProjectFiles.Resolve();
            if (testProjectFiles is null || !testProjectFiles.Any())
            {
                context.Log.Warning("No solution or project files found to test.");
                return;
            }

            foreach (string testProjectFile in testProjectFiles)
                context.DotNetCoreTest(testProjectFile);
        }

        [Config]
        public static void ConfigureDotNetCore(this ICakeContext context, TaskConfig config)
        {
            string[] GetProjectFiles()
            {
                var env = config.Load<EnvConfig>();
                string workingDirectory = env.WorkingDirectory ?? Directory.GetCurrentDirectory();

                string[] projectFiles = Directory.GetFiles(workingDirectory, "*.sln", SearchOption.TopDirectoryOnly);
                if (projectFiles.Length == 0)
                    projectFiles = Directory.GetFiles(workingDirectory, "*.sln", SearchOption.AllDirectories);
                if (projectFiles.Length == 0)
                    projectFiles = Directory.GetFiles(workingDirectory, "*.csproj", SearchOption.AllDirectories);
                if (projectFiles.Length > 1)
                    return projectFiles;
                if (projectFiles.Length == 1)
                    return new[] { projectFiles[0] };
                return Array.Empty<string>();
            }

            var cfg = config.Load<DotNetCoreConfig>();
            cfg.BuildProjectFiles = (Func<IEnumerable<string>>)GetProjectFiles;
            cfg.TestProjectFiles = (Func<IEnumerable<string>>)GetProjectFiles;
        }
    }
}
