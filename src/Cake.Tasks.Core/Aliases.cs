// Cake Tasks framework for Cake Build
// Copyright (c) 2019-2022 Jeevan James
// This file is licensed to you under the Apache License, Version 2.0.
// See the LICENSE file in the project root for more information.

using System;

using Cake.Core;
using Cake.Core.Annotations;

// ReSharper disable once CheckNamespace
namespace Cake.Tasks.Config;

public static class Aliases
{
    [CakeMethodAlias]
    [CakeNamespaceImport(CakeNamespace)]
    public static void ConfigureTasks(this ICakeContext ctx, Action<TaskConfig> setter)
    {
        if (ctx is null)
            throw new ArgumentNullException(nameof(ctx));
        if (setter is null)
            throw new ArgumentNullException(nameof(setter));

        // Don't run this setter now. Wait for all the config tasks to be called to initialize
        // the configs, and then call this to override with custom values.
        // The setter is actually called by calling TaskConfig.Current.PerformDeferredSetup at
        // various places in the TasksEngine.
        TaskConfig.Current.SetDeferredSetup(setter);
    }

    [CakeMethodAlias]
    [CakeNamespaceImport(CakeNamespace)]
    public static void ConfigureTask<TConfig>(this ICakeContext ctx, Action<TConfig> setter)
        where TConfig : PluginConfig
    {
        if (ctx is null)
            throw new ArgumentNullException(nameof(ctx));
        if (setter is null)
            throw new ArgumentNullException(nameof(setter));

        ConfigureTasks(ctx, config =>
        {
            TConfig cfg = config.Load<TConfig>();
            setter(cfg);
        });
    }

    [CakeMethodAlias]
    [CakeNamespaceImport(CakeNamespace)]
    public static void ConfigureTask<TConfig>(this ICakeContext ctx, Action<TConfig, TaskConfig> setter)
        where TConfig : PluginConfig
    {
        if (ctx is null)
            throw new ArgumentNullException(nameof(ctx));
        if (setter is null)
            throw new ArgumentNullException(nameof(setter));

        ConfigureTasks(ctx, config =>
        {
            TConfig cfg = config.Load<TConfig>();
            setter(cfg, config);
        });
    }

    private const string CakeNamespace = "Cake.Tasks.Config";
}
