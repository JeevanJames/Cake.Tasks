#region --- License & Copyright Notice ---
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
using Cake.Common;
using Cake.Common.Tools.DotNetCore;
using Cake.Common.Tools.DotNetCore.NuGet.Push;
using Cake.Common.Tools.DotNetCore.Pack;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Tasks.Config;
using Cake.Tasks.Core;
using Cake.Tasks.JeevanJames;

[assembly: TaskPlugin(typeof(JeevanJamesTasks))]

namespace Cake.Tasks.JeevanJames
{
    public static class JeevanJamesTasks
    {
        [PipelineTask(PipelineTask.Deploy, CiSystem = "appveyor")]
        public static void DeployToMyGet(ICakeContext ctx, TaskConfig cfg)
        {
            var jj = cfg.Load<JeevanJamesConfig>();
            var env = cfg.Load<EnvConfig>();

            IList<PublishProfile> profiles = jj.PublishProfiles.Resolve();
            foreach (PublishProfile profile in profiles)
                profile.OutputDirectory = ctx.Pack(profile, env);
            foreach (PublishProfile profile in profiles)
                ctx.Push(profile, cfg);
        }

        private static string Pack(this ICakeContext ctx, PublishProfile profile, EnvConfig env)
        {
            string outputDirectory = Path.Combine(env.Directories.BinaryOutput, profile.Name);

            ctx.DotNetCorePack(profile.ProjectFile, new DotNetCorePackSettings
            {
                IncludeSource = true,
                IncludeSymbols = true,
                Configuration = env.Configuration,
                OutputDirectory = outputDirectory,
                ArgumentCustomization = arg => arg.Append($"/p:Version={env.Version.Build}"),
            });

            return Directory.EnumerateFiles(outputDirectory, "*.nupkg").Single();
        }

        private static void Push(this ICakeContext ctx, PublishProfile profile, TaskConfig cfg)
        {
            var jj = cfg.Load<JeevanJamesConfig>();
            ctx.DotNetCoreNuGetPush(profile.OutputDirectory, new DotNetCoreNuGetPushSettings
            {
                ApiKey = jj.ApiKey,
                Source = jj.Source,
            });
        }
    }
}
