using System;
using Cake.Common.Tools.OctopusDeploy;
using Cake.Core;
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

            var settings = new OctopusPackSettings
            {
                BasePath = octopus.Pack.BasePath.Resolve(),
                Format = OctopusPackFormat.Zip,
                OutFolder = octopus.Pack.OutFolder.Resolve(),
            };
            if (octopus.Pack.Version != null)
                settings.Version = octopus.Pack.Version;

            ctx.OctoPack(octopus.PackageId, settings);

            //ctx.OctoPush("Octopus_Url", "Octopus_ApiKey");
        }

        [Config]
        public static void ConfigureOctopus(ICakeContext ctx, TaskConfig cfg)
        {
            var octopus = cfg.Load<OctopusConfig>();
            var env = cfg.Load<EnvConfig>();
            var ci = cfg.Load<CiConfig>();

            octopus.PackageId = null;

            octopus.Pack.BasePath = env.WorkingDirectory;
            octopus.Pack.OutFolder = ci.ArtifactsDirectory;
            octopus.Pack.Version = null;
        }
    }
}
