﻿using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Tasks.Core;
using Cake.Tasks.DotNetCore;

[assembly: TaskPlugin(typeof(DotNetCoreTasks))]

namespace Cake.Tasks.DotNetCore
{
    public static class DotNetCoreTasks
    {
        [CoreTask(CoreTasks.Build)]
        public static void Build(this ICakeContext context)
        {
            context.Log.Information(".NET Core build task");
        }
    }
}
