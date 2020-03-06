#region --- License & Copyright Notice ---
/*
Cake Tasks
Copyright 2019 Jeevan James

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Cake.Common.Tools.DotNetCore;
using Cake.Common.Tools.DotNetCore.Build;
using Cake.Common.Tools.DotNetCore.Clean;
using Cake.Common.Tools.DotNetCore.Pack;
using Cake.Common.Tools.DotNetCore.Publish;
using Cake.Common.Tools.DotNetCore.Test;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Tasks.Config;
using Cake.Tasks.Core;
using Cake.Tasks.DotNetCore;

using DotNetCorePublisher = Cake.Tasks.Config.DotNetCorePublisher;

[assembly: TaskPlugin(typeof(DotNetCoreTasks))]

namespace Cake.Tasks.DotNetCore
{
    public static class DotNetCoreTasks
    {
        [BeforePipelineTask(PipelineTask.Build)]
        public static void CleanDotNetCoreSolution(ICakeContext context, TaskConfig config)
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

            string outputDirectory = Path.Combine(env.Directories.BinaryOutput, "__build");

            context.DotNetCoreBuild(buildProjectFile, new DotNetCoreBuildSettings
            {
                OutputDirectory = outputDirectory,
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
                OutputDirectory = Path.Combine(env.Directories.BinaryOutput, "__build"),
                Logger = "trx",
                ResultsDirectory = env.Directories.TestOutput,
                NoBuild = true,
                NoRestore = true,
                Verbosity = context.Log.Verbosity.ToVerbosity(),
                ArgumentCustomization = pab => pab
                    .Append("/p:CollectCoverage=true")
                    .Append($"/p:CoverletOutput={Path.Combine(env.Directories.TestOutput, "coverage")}")
                    .Append("/p:CoverletOutputFormat=opencover")
                    .Append("/p:Exclude=[xunit.*]*"),
            };
            string filter = test.Filter;
            if (!string.IsNullOrWhiteSpace(filter))
                settings.Filter = filter;

            context.DotNetCoreTest(testProjectFile, settings);
        }

        [AfterPipelineTask(PipelineTask.Test)]
        public static void ShutDownBuildServer(ICakeContext ctx, TaskConfig cfg)
        {
            ctx.DotNetCoreBuildServerShutdown();
        }

        [BeforePipelineTask(PipelineTask.Deploy)]
        public static void PublishDotNetProjects(ICakeContext ctx, TaskConfig config)
        {
            var env = config.Load<EnvConfig>();

            IList<DotNetCorePublisher> publishers = env.Publishers.OfType<DotNetCorePublisher>().ToList();
            if (publishers.Count == 0)
                ctx.Log.Information("No .NET Core projects to publish. Specify a publisher.");
            foreach (DotNetCorePublisher publisher in publishers)
            {
                string publishDirectory = Path.Combine(env.Directories.PublishOutput, publisher.Name);
                if (!Directory.Exists(publishDirectory))
                    Directory.CreateDirectory(publishDirectory);
                publisher.SetOutput(publishDirectory);
                switch (publisher)
                {
                    case AspNetCorePublisher aspnet:
                        PublishAspNetCore(ctx, config, aspnet);
                        break;
                    case NuGetPublisher nuget:
                        PublishNuGet(ctx, config, nuget);
                        break;
                    default:
                        throw new TaskConfigException($"Unrecognized .NET Core publisher type: {publisher.GetType().FullName}.");
                }
            }
        }

        private static void PublishAspNetCore(ICakeContext ctx, TaskConfig cfg, AspNetCorePublisher publisher)
        {
            var env = cfg.Load<EnvConfig>();

            ctx.DotNetCorePublish(publisher.ProjectFile, new DotNetCorePublishSettings
            {
                Configuration = env.Configuration,
                OutputDirectory = publisher.OutputLocation,
                Verbosity = ctx.Log.Verbosity.ToVerbosity(),
                ArgumentCustomization = arg => arg.Append($"/p:Version={env.Version.Build}"),
            });
        }

        private static void PublishNuGet(ICakeContext ctx, TaskConfig cfg, NuGetPublisher nuget)
        {
            var env = cfg.Load<EnvConfig>();

            ctx.DotNetCorePack(nuget.ProjectFile, new DotNetCorePackSettings
            {
                Configuration = env.Configuration,
                OutputDirectory = nuget.OutputLocation,
                Verbosity = ctx.Log.Verbosity.ToVerbosity(),
                ArgumentCustomization = arg => arg.Append($"/p:Version={env.Version.Build}"),
                IncludeSource = true,
                IncludeSymbols = true,
            });
        }

        [Config]
        public static void ConfigureDotNetCore(ICakeContext context, TaskConfig config)
        {
            var cfg = config.Load<DotNetCoreConfig>();

            var env = config.Load<EnvConfig>();
            string workingDirectory = env.Directories.Working ?? Directory.GetCurrentDirectory();

            cfg.Build.ProjectFile = (Func<string>)(() => GetBuildProjectFile(workingDirectory));

            cfg.Test.Skip = false;
            cfg.Test.ProjectFile = (Func<string>)(() => GetBuildProjectFile(workingDirectory));

            static string GetBuildProjectFile(string wdir)
            {
                return Directory
                    .GetFiles(wdir, "*.sln", SearchOption.TopDirectoryOnly)
                    .FirstOrDefault();
            }
        }

        private static DotNetCoreVerbosity ToVerbosity(this Verbosity verbosity)
        {
            return verbosity switch
            {
                Verbosity.Diagnostic => DotNetCoreVerbosity.Diagnostic,
                Verbosity.Minimal => DotNetCoreVerbosity.Minimal,
                Verbosity.Normal => DotNetCoreVerbosity.Normal,
                Verbosity.Quiet => DotNetCoreVerbosity.Quiet,
                Verbosity.Verbose => DotNetCoreVerbosity.Detailed,
                _ => DotNetCoreVerbosity.Normal,
            };
        }
    }
}
