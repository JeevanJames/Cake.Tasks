using System;
using System.Linq;
using Cake.Common.Tools.OctopusDeploy;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using Cake.Tasks.Config;
using Cake.Tasks.Core;
using Cake.Tasks.Octopus;

[assembly: TaskPlugin(typeof(OctopusTasks))]

namespace Cake.Tasks.Octopus
{
    public static class OctopusTasks
    {
        [CoreTask(CoreTask.Deploy)]
        public static void Deploy(ICakeContext ctx, TaskConfig cfg)
        {
            var octopus = cfg.Load<OctopusConfig>();

            if (string.IsNullOrWhiteSpace(octopus.PackageId))
                throw new TaskConfigException("Configure a package ID for the Octopus deployment.");

            ctx.Log.Information("Creating Octopus package");

            var settings = new OctopusPackSettings
            {
                BasePath = octopus.Pack.BasePath.Resolve(),
                Format = OctopusPackFormat.Zip,
                OutFolder = octopus.Pack.OutFolder.Resolve(),
            };
            if (octopus.Pack.Version != null)
                settings.Version = octopus.Pack.Version;

            ctx.OctoPack(octopus.PackageId, settings);

            ctx.Log.Information("Pushing Octopus package");

            string packageFile = System.IO.Directory.GetFiles(octopus.Pack.OutFolder, $"{octopus.PackageId}*.zip").FirstOrDefault();
            ctx.Log.Information($"API Key: {octopus.ApiKey}");
            ctx.OctoPush(octopus.Server, octopus.ApiKey, new FilePath(packageFile), new OctopusPushSettings
            {
                ApiKey = octopus.ApiKey,
            });

            ctx.Log.Information("Creating Octopus release");

            var releaseSettings = new CreateReleaseSettings
            {
                ApiKey = octopus.ApiKey,
                Server = octopus.Server,
                DeployTo = octopus.Release.DeployTo,
            };
            ctx.OctoCreateRelease(octopus.Release.ProjectName, releaseSettings);
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
