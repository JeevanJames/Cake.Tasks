using System.Linq;
using Cake.Common;
using Cake.Core;
using Cake.Tasks.Config;
using Cake.Tasks.Core;
using Cake.Tasks.Env.Tfs;

[assembly: TaskPlugin(typeof(TfsTasks))]

namespace Cake.Tasks.Env.Tfs
{
    public static class TfsTasks
    {
        [Config(Environment = "tfs", Order = 1)]
        public static void CleanBuildArtifacts(ICakeContext ctx, TaskConfig cfg)
        {
            var ci = cfg.Load<CiConfig>();
            ci.Version = ctx.EnvironmentVariable("BUILD_BUILDNUMBER", "1");

            string buildNum = ci.Version.Split('.').LastOrDefault() ?? "1";
            ci.BuildNumber = int.TryParse(buildNum, out int bn) ? bn : 1;

            string artifactsDir = ctx.EnvironmentVariable("BUILD_ARTIFACTSTAGINGDIRECTORY");
            if (!string.IsNullOrWhiteSpace(artifactsDir))
                ci.ArtifactsDirectory = artifactsDir;

            string binaryArtifactsDir = ctx.EnvironmentVariable("BUILD_BINARIESDIRECTORY");
            if (!string.IsNullOrWhiteSpace(binaryArtifactsDir))
                ci.ArtifactsDirectory = binaryArtifactsDir;
        }
    }
}
