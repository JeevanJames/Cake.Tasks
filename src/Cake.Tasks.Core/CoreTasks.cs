﻿#region --- License & Copyright Notice ---
/*
Cake Tasks
Copyright 2019 Jeevan James

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/
#endregion

using System.Collections.Generic;
using System.IO;
using System.Linq;

using Cake.Core;
using Cake.Tasks.Config;
using Cake.Tasks.Core;

[assembly: TaskPlugin(typeof(CoreTasks))]

namespace Cake.Tasks.Core
{
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

            var directoryPublishers = env.Publishers.OfType<DirectoryPublisher>().ToList();
            foreach (DirectoryPublisher publisher in directoryPublishers)
            {
                publisher.ValidateDirectory();

                string directory = Path.Combine(env.Directories.Working, publisher.Directory);
                publisher.SetOutput(directory);
            }
        }
    }
}
