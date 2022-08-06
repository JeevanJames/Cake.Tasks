// Cake Tasks framework for Cake Build
// Copyright (c) 2019-2022 Jeevan James
// This file is licensed to you under the Apache License, Version 2.0.
// See the LICENSE file in the project root for more information.

// ReSharper disable once CheckNamespace
namespace Cake.Tasks.Config;

public sealed class GitVersionConfig : PluginConfig
{
    public GitVersionConfig(TaskConfig taskConfig)
        : base(taskConfig)
    {
    }

    public Common.Tools.GitVersion.GitVersion Version
    {
        get => Get<Common.Tools.GitVersion.GitVersion>(Keys.Version);
        set => Set(Keys.Version, value);
    }

    public static class Keys
    {
        public const string Version = "GitVersion_Version";
    }
}
