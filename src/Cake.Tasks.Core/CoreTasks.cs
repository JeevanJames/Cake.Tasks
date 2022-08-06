// Cake Tasks framework for Cake Build
// Copyright (c) 2019-2022 Jeevan James
// This file is licensed to you under the Apache License, Version 2.0.
// See the LICENSE file in the project root for more information.

using System.IO;
using System.Linq;

using Cake.Core;
using Cake.Tasks.Config;
using Cake.Tasks.Core;

[assembly: TaskPlugin(typeof(CoreTasks))]

namespace Cake.Tasks.Core;

public static class CoreTasks
{
    /// <summary>
    ///     Sets the output details for any <see cref="DirectoryPublisher"/> publishers or derived
    ///     publishers. It does this by resolving the final directory path and calling the
    ///     <see cref="Publisher.SetOutput(string, PublishOutputType)"/> method on the publisher.
    /// </summary>
    /// <param name="ctx">The <see cref="ICakeContext"/>.</param>
    /// <param name="cfg">The <see cref="TaskConfig"/>.</param>
    [BeforePipelineTask(PipelineTask.Build)]
    public static void SetDirectoryPublisherOutputs(ICakeContext ctx, TaskConfig cfg)
    {
        EnvConfig env = cfg.Load<EnvConfig>();

        foreach (DirectoryPublisher publisher in env.Publishers.OfType<DirectoryPublisher>().ToList())
        {
            publisher.ValidateDirectory();

            string directory = Path.Combine(env.Directories.Working, publisher.Directory);
            publisher.SetOutput(directory);
        }
    }
}
