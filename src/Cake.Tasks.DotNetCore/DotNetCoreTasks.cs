using System;
using System.Collections.Generic;
using System.IO;
using Cake.Common.Tools.DotNetCore;
using Cake.Core;
using Cake.Core.Diagnostics;
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
            string cleanProjectFile = cfg.BuildProjectFile.Resolve();
            if (cleanProjectFile is null)
            {
                context.Log.Warning("No solution or project files found to clean.");
                return;
            }

            context.DotNetCoreClean(cleanProjectFile);
        }

        [CoreTask(CoreTask.Build)]
        public static void Build(this ICakeContext context, TaskConfig config)
        {
            var cfg = config.Load<DotNetCoreConfig>();
            string buildProjectFile = cfg.BuildProjectFile.Resolve();
            if (buildProjectFile is null)
            {
                context.Log.Warning("No solution or project files found to build.");
                return;
            }

            context.DotNetCoreBuild(buildProjectFile);
        }

        [CoreTask(CoreTask.Test)]
        public static void Test(this ICakeContext context, TaskConfig config)
        {
            var cfg = config.Load<DotNetCoreConfig>();
            string testProjectFile = cfg.BuildProjectFile.Resolve();
            if (testProjectFile is null)
            {
                context.Log.Warning("No solution or project files found to test.");
                return;
            }

            context.DotNetCoreTest(testProjectFile);
        }

        [Config]
        public static void Configuration(this ICakeContext context, TaskConfig config)
        {
            string GetProjectFiles()
            {
                var env = config.Load<EnvConfig>();
                string workingDirectory = env.WorkingDirectory ?? Directory.GetCurrentDirectory();

                string[] projectFiles = Directory.GetFiles(workingDirectory, "*.sln", SearchOption.TopDirectoryOnly);
                if (projectFiles.Length == 0)
                    projectFiles = Directory.GetFiles(workingDirectory, "*.sln", SearchOption.AllDirectories);
                if (projectFiles.Length == 0)
                    projectFiles = Directory.GetFiles(workingDirectory, "*.csproj", SearchOption.AllDirectories);
                if (projectFiles.Length > 1)
                    return projectFiles[0];
                if (projectFiles.Length == 1)
                    return projectFiles[0];
                return null;
            }

            var cfg = config.Load<DotNetCoreConfig>();
            cfg.BuildProjectFile = new TaskConfigValue<string>(GetProjectFiles);
        }
    }
}
