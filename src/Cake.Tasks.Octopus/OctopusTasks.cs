using Cake.Core;
using Cake.Core.Diagnostics;
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
            ctx.Log.Information("In Octopus Deploy");
        }
    }
}
