// Cake Tasks framework for Cake Build
// Copyright (c) 2019-2022 Jeevan James
// This file is licensed to you under the Apache License, Version 2.0.
// See the LICENSE file in the project root for more information.

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
        [Config(Order = ConfigTaskOrder.Priority)]
        public static void ConfigureGitVersion(ICakeContext ctx, TaskConfig cfg)
        {
            // Update the GitVersion config
            GitVersionConfig gitVersion = cfg.Load<GitVersionConfig>();
            Common.Tools.GitVersion.GitVersion version = ctx.GitVersion();
            gitVersion.Version = version;

            EnvConfig env = cfg.Load<EnvConfig>();

            // Update repository details
            env.Repository.Branch = version.BranchName;
            env.Repository.Commit = version.Sha;

            // Update primary and full versions
            env.Version.Primary = version.MajorMinorPatch;
            env.Version.Full = version.SemVer;

            // Update build number
            env.Version.BuildNumber = version.CommitsSinceVersionSource.HasValue
                ? version.CommitsSinceVersionSource.Value.ToString()
                : version.BuildMetaData;
        }
    }
}
