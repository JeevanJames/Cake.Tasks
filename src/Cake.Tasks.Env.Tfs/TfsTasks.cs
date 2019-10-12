using System.Linq;

using Cake.Common.Build;
using Cake.Common.Build.TFBuild;
using Cake.Core;
using Cake.Tasks.Config;
using Cake.Tasks.Core;
using Cake.Tasks.Env.Tfs;

[assembly: TaskPlugin(typeof(TfsTasks))]

namespace Cake.Tasks.Env.Tfs
{
    public static class TfsTasks
    {
        [Config(CiSystem = "tfs", Order = 1)]
        public static void ConfigureTfsEnvironment(ICakeContext ctx, TaskConfig cfg)
        {
            ITFBuildProvider tfs = ctx.TFBuild();
            if (!tfs.IsRunningOnAzurePipelines)
                throw new TaskConfigException("The CI/CD process is not running in TFS or Azure Pipelines");

            var env = cfg.Load<EnvConfig>();
            env.IsCi = true;

            var ci = cfg.Load<CiConfig>();
            ci.BuildNumber = int.TryParse(tfs.Environment.Build.Number, out int buildNum)
                ? buildNum
                : tfs.Environment.Build.Id;
            ci.Version = $"1.0.0-rev.{ci.BuildNumber}";

            ci.ArtifactsDirectory = tfs.Environment.Build.ArtifactStagingDirectory.FullPath;
            ci.BuildOutputDirectory = tfs.Environment.Build.BinariesDirectory.FullPath;
            ci.TestOutputDirectory = System.IO.Path.Combine(tfs.Environment.Build.SourcesDirectory.FullPath, "TestResults");
        }

        //[TaskEvent(TaskEventType.AfterTask, CoreTask.Test, Environment = "tfs")]
        //public static void PublishTestResults(ICakeContext ctx, TaskConfig cfg)
        //{
        //    ITFBuildProvider tfs = ctx.TFBuild();
        //    if (!tfs.IsRunningOnAzurePipelines)
        //        return;

        //    var ci = cfg.Load<CiConfig>();
        //    List<FilePath> trxFiles = ctx.Globber.Match(
        //            System.IO.Path.Combine(ci.TestOutputDirectory, "*.trx"))
        //        .OfType<FilePath>()
        //        .ToList();

        //    var data = new TFBuildPublishTestResultsData
        //    {
        //        MergeTestResults = true,
        //        TestRunner = TFTestRunnerType.VSTest,
        //        TestResultsFiles = trxFiles,
        //        TestRunTitle = "Cake.Tasks Test Results",
        //    };
        //    tfs.Commands.PublishTestResults(data);
        //}
    }
}
