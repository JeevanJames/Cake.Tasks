using Cake.Common.IO;
using Cake.Core;
using Cake.Tasks.Config;
using Cake.Tasks.Core;
using Cake.Tasks.Local;

[assembly: TaskPlugin(typeof(LocalTasks))]

namespace Cake.Tasks.Local
{
    public static class LocalTasks
    {
        [TaskEvent(TaskEventType.BeforeTask, PipelineTask.Build)]
        public static void CleanArtifactsDirectory(ICakeContext ctx, TaskConfig cfg)
        {
            var env = cfg.Load<EnvConfig>();
            if (env.IsCi)
                return;

            var ci = cfg.Load<CiConfig>();
            if (ctx.DirectoryExists(ci.ArtifactsDirectory))
            {
                ctx.DeleteDirectory(ci.ArtifactsDirectory, new DeleteDirectorySettings
                {
                    Recursive = true,
                });
            }
        }
    }
}
