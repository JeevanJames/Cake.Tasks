// Cake Tasks framework for Cake Build
// Copyright (c) 2019-2022 Jeevan James
// This file is licensed to you under the Apache License, Version 2.0.
// See the LICENSE file in the project root for more information.

using Cake.Common.Build;
using Cake.Common.Build.AppVeyor;
using Cake.Core;
using Cake.Tasks.Ci.AppVeyor;
using Cake.Tasks.Config;
using Cake.Tasks.Core;

[assembly: TaskPlugin(typeof(CiAppVeyorTasks))]

namespace Cake.Tasks.Ci.AppVeyor;

public static class CiAppVeyorTasks
{
    [Config(CiSystem = "appveyor", Order = ConfigTaskOrder.CiSystem)]
    public static void ConfigureAppVeyorEnvironment(ICakeContext ctx, TaskConfig cfg)
    {
        IAppVeyorProvider appveyor = ctx.AppVeyor();

        if (!appveyor.IsRunningOnAppVeyor)
            throw new TaskConfigException("Not running in AppVeyor");

        EnvConfig env = cfg.Load<EnvConfig>();
        env.IsCi = true;

        env.Repository.Name = appveyor.Environment.Repository.Name;
        env.Repository.Type = appveyor.Environment.Repository.Scm;
        env.Repository.Branch = appveyor.Environment.Repository.Branch;
        env.Repository.Commit = appveyor.Environment.Repository.Commit.Id;

        env.Version.BuildNumber = appveyor.Environment.Build.Number.ToString();
        env.Version.Build = $"{env.Version.Primary}-build.{env.Version.BuildNumber}";
    }
}
