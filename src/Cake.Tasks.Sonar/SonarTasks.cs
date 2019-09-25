using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Tasks.Config;
using Cake.Tasks.Core;
using Cake.Tasks.Sonar;

[assembly: TaskPlugin(typeof(SonarTasks))]

namespace Cake.Tasks.Sonar
{
    public static class SonarTasks
    {
        [TaskEvent(TaskEventType.BeforeTask, CoreTask.Build)]
        public static void StartScanner(ICakeContext ctx, TaskConfig cfg)
        {
            var sonar = cfg.Load<SonarConfig>();
            ctx.Log.Information($"Starting Sonar scanner on {sonar.Url}");
        }

        [TaskEvent(TaskEventType.AfterTask, CoreTask.Test)]
        public static void StopScanner(ICakeContext ctx, TaskConfig cfg)
        {
            var sonar = cfg.Load<SonarConfig>();
            ctx.Log.Information($"Stopping Sonar scanner on {sonar.Url}");
        }

        [Config]
        public static void ConfigureSonar(ICakeContext ctx, TaskConfig cfg)
        {
            var sonar = cfg.Load<SonarConfig>();
            sonar.Url = "http://localhost:9000";
        }
    }
}
