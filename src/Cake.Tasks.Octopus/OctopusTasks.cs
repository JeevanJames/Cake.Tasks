﻿using System;
using System.Linq;
using Cake.Common.Tools.OctopusDeploy;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using Cake.Tasks.Config;
using Cake.Tasks.Core;
using Cake.Tasks.Octopus;
using Path = System.IO.Path;

[assembly: TaskPlugin(typeof(OctopusTasks))]

namespace Cake.Tasks.Octopus
{
    public static class OctopusTasks
    {
        [PipelineTask(PipelineTask.Deploy)]
        public static void Deploy(ICakeContext ctx, TaskConfig cfg)
        {
            var octopus = cfg.Load<OctopusConfig>();
            var ci = cfg.Load<CiConfig>();

            if (string.IsNullOrWhiteSpace(octopus.PackageId))
                throw new TaskConfigException("Configure a package ID for the Octopus deployment.");

            ctx.Log.Information("Creating Octopus package.");

            var settings = new OctopusPackSettings
            {
                BasePath = Path.Combine(ci.BuildOutputDirectory, "publish"),
                Format = OctopusPackFormat.Zip,
                OutFolder = Path.Combine(ci.ArtifactsDirectory),
            };
            if (octopus.Pack.Version != null)
                settings.Version = octopus.Pack.Version;
            else
                settings.Version = ci.Version;

            ctx.OctoPack(octopus.PackageId, settings);

            //ctx.Log.Information("Pushing Octopus package");

            //string packageFileName = $"{octopus.PackageId}.{ci.Version}.zip";
            //string packageFile = Path.Combine(octopus.Pack.OutFolder, packageFileName);
            //ctx.OctoPush(octopus.Server, octopus.ApiKey, new FilePath(packageFile), new OctopusPushSettings
            //{
            //    ApiKey = octopus.ApiKey,
            //});

            //ctx.Log.Information("Creating Octopus release");

            //var releaseSettings = new CreateReleaseSettings
            //{
            //    ApiKey = octopus.ApiKey,
            //    Server = octopus.Server,
            //    DeployTo = octopus.Release.DeployTo,
            //};
            //ctx.OctoCreateRelease(octopus.Release.ProjectName, releaseSettings);
        }

        [Config]
        public static void ConfigureOctopus(ICakeContext ctx, TaskConfig cfg)
        {
            var octopus = cfg.Load<OctopusConfig>();
            var env = cfg.Load<EnvConfig>();
            var ci = cfg.Load<CiConfig>();

            octopus.Server = "http://localhost";
            octopus.ApiKey = null;
            octopus.PackageId = null;

            octopus.Pack.BasePath = (Func<string>)(() => env.WorkingDirectory);
            octopus.Pack.OutFolder = (Func<string>)(() => ci.ArtifactsDirectory);
            octopus.Pack.Version = null;

            octopus.Release.ProjectName = null;
            octopus.Release.DeployTo = null;
        }
    }
}
