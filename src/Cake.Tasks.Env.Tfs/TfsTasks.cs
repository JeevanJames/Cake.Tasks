using Cake.Core;
using Cake.Tasks.Config;
using Cake.Tasks.Core;

namespace Cake.Tasks.Env.Tfs
{
    public static class TfsTasks
    {
        [TaskEvent(TaskEventType.BeforeTask, CoreTask.Build)]
        public static void CleanBuildArtifacts(ICakeContext ctx, TaskConfig cfg)
        {
            //TODO:
        }
    }
}
