using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Sonar;
using Cake.Tasks.Config;
using Cake.Tasks.Core;
using Cake.Tasks.Sonar;

[assembly: TaskPlugin(typeof(SonarTasks))]

namespace Cake.Tasks.Sonar
{
    public static class SonarTasks
    {
        [TaskEvent(TaskEventType.BeforeTask, PipelineTask.Build)]
        public static void StartScanner(ICakeContext ctx, TaskConfig config)
        {
            var sonar = config.Load<SonarConfig>();

            ctx.SonarBegin(new SonarBeginSettings
            {
                Key = sonar.Key,
                Login = sonar.Login,
                OpenCoverReportsPath = sonar.OpenCoverReportsPath,
                TestReportPaths = sonar.TestReportPaths,
                Url = sonar.Url,
                Verbose = ctx.Log.Verbosity >= Verbosity.Verbose,
            });
        }

        [TaskEvent(TaskEventType.AfterTask, PipelineTask.Test)]
        public static void StopScanner(ICakeContext ctx, TaskConfig cfg)
        {
            var sonar = cfg.Load<SonarConfig>();

            ctx.SonarEnd(new SonarEndSettings
            {
                Login = sonar.Login,
            });
        }

        [Config]
        public static void ConfigureSonar(ICakeContext ctx, TaskConfig cfg)
        {
            var sonar = cfg.Load<SonarConfig>();
            sonar.Key = null;
            sonar.Login = null;
            sonar.OpenCoverReportsPath = null;
            sonar.TestReportPaths = null;
            sonar.Url = "http://localhost:9000";
        }
    }
}
