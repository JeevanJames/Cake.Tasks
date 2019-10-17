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

using System;
using Cake.Common.Tools.GitVersion;
using Cake.Core;
using Cake.Tasks.Config;
using Cake.Tasks.Core;
using Cake.Tasks.GitVersion;

[assembly: TaskPlugin(typeof(GitVersionTasks))]

namespace Cake.Tasks.GitVersion
{
    public static class GitVersionTasks
    {
        [Config(Order = 100)]
        public static void ConfigureGitVersion(ICakeContext ctx, TaskConfig cfg)
        {
            var env = cfg.Load<EnvConfig>();
            var gitVersion = cfg.Load<GitVersionConfig>();

            Common.Tools.GitVersion.GitVersion version = ctx.GitVersion();

            gitVersion.Version = version;

            env.Version = version.MajorMinorPatch;
            env.FullVersion = version.FullSemVer;

            if (version.CommitsSinceVersionSource.HasValue)
                env.BuildNumber = version.CommitsSinceVersionSource.Value;
            else if (int.TryParse(version.BuildMetaData, out int buildMetadata))
                env.BuildNumber = buildMetadata;
            else
                env.BuildNumber = 1;
        }
    }
}
