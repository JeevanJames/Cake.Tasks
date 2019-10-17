using System;
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
        [TaskEvent(TaskEventType.BeforeTask, PipelineTask.Build)]
        public static void BeforeDotNetCoreBuild(ICakeContext context, TaskConfig config)
        {
            context.DotNetCoreBuildServerShutdown();

            var cfg = config.Load<DotNetCoreConfig>();
            string cleanProjectFile = cfg.Build.ProjectFile.Resolve();
            if (cleanProjectFile is null)
                throw new TaskConfigException("Build solution or project file not specified.");

            context.DotNetCoreClean(cleanProjectFile, new DotNetCoreCleanSettings
            {
                Verbosity = context.Log.Verbosity.ToVerbosity(),
            });
        }

        [PipelineTask(PipelineTask.Build)]
        public static void BuildDotNetCoreSolution(ICakeContext context, TaskConfig config)
        {
            var build = config.Load<DotNetCoreConfig>().Build;

            string buildProjectFile = build.ProjectFile;
            if (buildProjectFile is null)
                throw new TaskConfigException("Build solution or project file not specified.");

            var env = config.Load<EnvConfig>();

            context.DotNetCoreBuild(buildProjectFile, new DotNetCoreBuildSettings
            {
                OutputDirectory = env.Directories.BuildOutput,
                Configuration = env.Configuration,
                Verbosity = context.Log.Verbosity.ToVerbosity(),
            });
        }

        [PipelineTask(PipelineTask.Test)]
        public static void TestDotNetCoreProjects(ICakeContext context, TaskConfig config)
        {
            var test = config.Load<DotNetCoreConfig>().Test;

            if (test.Skip)
            {
                context.Log.Information("Skipping tests.");
                return;
            }

            var env = config.Load<EnvConfig>();

            string testProjectFile = test.ProjectFile;
            if (testProjectFile is null)
                throw new TaskConfigException("Build solution or project file not specified.");

            var settings = new DotNetCoreTestSettings
            {
                Configuration = env.Configuration,
                OutputDirectory = env.Directories.BuildOutput,
                Logger = "trx",
                ResultsDirectory = env.Directories.TestOutput,
                NoBuild = true,
                NoRestore = true,
                Verbosity = context.Log.Verbosity.ToVerbosity(),
                ArgumentCustomization = pab => pab
                    .Append("/p:CollectCoverage=true")
                    .Append($"/p:CoverletOutput={Path.Combine(env.Directories.TestOutput, "coverage")}")
                    .Append("/p:CoverletOutputFormat=cobertura")
                    .Append("/p:CoverletOutputFormat=opencover")
                    .Append("/p:Exclude=[xunit.*]*"),
            };
            string filter = test.Filter;
            if (!string.IsNullOrWhiteSpace(filter))
                settings.Filter = filter;

            context.DotNetCoreTest(testProjectFile, settings);
        }

        [TaskEvent(TaskEventType.AfterTask, PipelineTask.Test)]
        public static void ShutDownBuildServer(ICakeContext ctx, TaskConfig cfg)
        {
            ctx.DotNetCoreBuildServerShutdown();
        }

        [TaskEvent(TaskEventType.BeforeTask, PipelineTask.Deploy)]
        public static void PublishDotNetProjects(ICakeContext ctx, TaskConfig config)
        {
            var cfg = config.Load<DotNetCoreConfig>().Publish;
            var env = config.Load<EnvConfig>();

            ctx.DotNetCorePublish(cfg.ProjectFile, new DotNetCorePublishSettings
            {
                Configuration = env.Configuration,
                OutputDirectory = Path.Combine(env.Directories.BuildOutput, "publish"),
                Verbosity = ctx.Log.Verbosity.ToVerbosity(),
                ArgumentCustomization = arg => arg.Append($"/p:Version={env.Version}"),
            });
        }

        [Config]
        public static void ConfigureDotNetCore(ICakeContext context, TaskConfig config)
        {
            var cfg = config.Load<DotNetCoreConfig>();

            var env = config.Load<EnvConfig>();
            string workingDirectory = env.Directories.Working ?? Directory.GetCurrentDirectory();

            string GetBuildProjectFile()
            {
                return Directory
                    .GetFiles(workingDirectory, "*.sln", SearchOption.TopDirectoryOnly)
                    .FirstOrDefault();
            }

            cfg.Build.ProjectFile = (Func<string>)GetBuildProjectFile;

            cfg.Test.Skip = false;
            cfg.Test.ProjectFile = (Func<string>)GetBuildProjectFile;

            cfg.Publish.ProjectFile = (Func<string>)GetBuildProjectFile;
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
