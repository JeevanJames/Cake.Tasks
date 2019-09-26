using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Cake.Common.Tools.DotNetCore;
using Cake.Common.Tools.DotNetCore.Build;
using Cake.Common.Tools.DotNetCore.Clean;
using Cake.Common.Tools.DotNetCore.Publish;
using Cake.Common.Tools.DotNetCore.Test;
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
        public static void BeforeDotNetCoreBuild(ICakeContext context, TaskConfig config)
        {
            context.DotNetCoreBuildServerShutdown();

            var cfg = config.Load<DotNetCoreConfig>();
            IEnumerable<string> cleanProjectFiles = cfg.Build.ProjectFiles.Resolve();
            if (cleanProjectFiles is null || !cleanProjectFiles.Any())
            {
                context.Log.Warning("No solution or project files found to clean.");
                return;
            }

            foreach (string cleanProjectFile in cleanProjectFiles)
            {
                context.DotNetCoreClean(cleanProjectFile, new DotNetCoreCleanSettings
                {
                    Verbosity = context.Log.Verbosity.ToVerbosity(),
                });
            }
        }

        [CoreTask(CoreTask.Build)]
        public static void Build(ICakeContext context, TaskConfig config)
        {
            var build = config.Load<DotNetCoreConfig>().Build;
            var env = config.Load<EnvConfig>();

            List<string> buildProjectFiles = build.ProjectFiles;
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
                    NoRestore = build.NoRestore,
                    Verbosity = context.Log.Verbosity.ToVerbosity(),
                });
            }
        }

        [CoreTask(CoreTask.Test)]
        public static void Test(ICakeContext context, TaskConfig config)
        {
            var test = config.Load<DotNetCoreConfig>().Test;

            if (test.Skip)
            {
                context.Log.Information("Skipping tests.");
                return;
            }

            var env = config.Load<EnvConfig>();

            List<string> testProjectFiles = test.ProjectFiles;
            if (testProjectFiles is null || !testProjectFiles.Any())
            {
                context.Log.Warning("No solution or project files found to test.");
                return;
            }

            foreach (string testProjectFile in testProjectFiles)
            {
                context.DotNetCoreTest(testProjectFile, new DotNetCoreTestSettings
                {
                    Configuration = env.Configuration,
                    NoBuild = test.NoBuild,
                    NoRestore = test.NoRestore,
                    Verbosity = context.Log.Verbosity.ToVerbosity(),
                });
            }
        }

        [TaskEvent(TaskEventType.AfterTask, CoreTask.Test)]
        public static void AfterDotNetCoreTest(ICakeContext ctx, TaskConfig cfg)
        {
            ctx.DotNetCoreBuildServerShutdown();
        }

        [TaskEvent(TaskEventType.BeforeTask, CoreTask.Deploy)]
        public static void PublishDotNet(ICakeContext ctx, TaskConfig config)
        {
            var cfg = config.Load<DotNetCoreConfig>().Publish;
            var env = config.Load<EnvConfig>();

            ctx.DotNetCorePublish(cfg.ProjectFile, new DotNetCorePublishSettings
            {
                Configuration = env.Configuration,
                Verbosity = ctx.Log.Verbosity.ToVerbosity(),
            });
        }

        [Config]
        public static void ConfigureDotNetCore(ICakeContext context, TaskConfig config)
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

            cfg.Build.ProjectFiles = (Func<IEnumerable<string>>)GetProjectFiles;
            cfg.Build.NoRestore = false;

            cfg.Test.Skip = false;
            cfg.Test.ProjectFiles = (Func<IEnumerable<string>>)GetProjectFiles;
            cfg.Test.NoRestore = false;
            cfg.Test.NoBuild = false;

            cfg.Publish.ProjectFile = (Func<string>)(() =>
            {
                string[] projectFiles = GetProjectFiles();
                if (projectFiles != null && projectFiles.Length > 0)
                    return projectFiles[0];
                return null;
            });
        }

        private static DotNetCoreVerbosity ToVerbosity(this Verbosity verbosity)
        {
            switch (verbosity)
            {
                case Verbosity.Diagnostic: return DotNetCoreVerbosity.Diagnostic;
                case Verbosity.Minimal: return DotNetCoreVerbosity.Minimal;
                case Verbosity.Normal: return DotNetCoreVerbosity.Normal;
                case Verbosity.Quiet: return DotNetCoreVerbosity.Quiet;
                case Verbosity.Verbose: return DotNetCoreVerbosity.Detailed;
                default: return DotNetCoreVerbosity.Normal;
            }
        }
    }
}
