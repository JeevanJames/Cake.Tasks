// Cake Tasks framework for Cake Build
// Copyright (c) 2019-2022 Jeevan James
// This file is licensed to you under the Apache License, Version 2.0.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Cake.Tasks.Config;

public sealed class TaskConfig
{
    public static TaskConfig Current { get; } = new();

    private TaskConfig()
    {
    }

    /// <summary>
    ///     Loads the subset of configurations for a specific plugin configuration type.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of plugin configuration to load the configuration for.
    /// </typeparam>
    /// <returns>The <see cref="PluginConfig"/> instance for the subset of configurations.</returns>
    public T Load<T>()
        where T : PluginConfig
    {
        return (T)Activator.CreateInstance(typeof(T), this);
    }

    public T Get<TConfig, T>(Func<TConfig, T> func)
        where TConfig : PluginConfig
    {
        TConfig config = Load<TConfig>();
        return func(config);
    }

    public T Get<T>(string key, T defaultValue = default)
    {
        return Data.TryGetValue(key, out object value) ? (T)value : defaultValue;
    }

    public T GetValue<T>(string key, T defaultValue = default)
    {
        if (!Data.TryGetValue(key, out object value))
            return defaultValue;

        return value switch
        {
            ConfigValue<T> configValue => configValue.Resolve(),
            T typedValue => typedValue,
            _ => throw new InvalidCastException($"Cannot cast config key '{key}' object to type '{typeof(T)}'."),
        };
    }

    public TaskConfig For<TConfig>(Action<TConfig> action)
        where TConfig : PluginConfig
    {
        TConfig config = Load<TConfig>();
        action(config);
        return this;
    }

    /// <summary>
    ///     Gets the internal structure that maintains all task configurations.
    /// </summary>
    internal IDictionary<string, object> Data { get; } =
        new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

    private List<Action<TaskConfig>> _deferredSetups;

    /// <summary>
    ///     For any calls to <c>ConfigureTasks</c> or <c>ConfigureTask</c> in the .cake file, the
    ///     specified lambdas are stored in a list so that they can be executed at the last possible
    ///     time. This will enable the .cake configurations to override anything else.
    ///     <para/>
    ///     At the appropriate point in the framework, it will call the <see cref="PerformDeferredSetup"/>
    ///     method to execute the lambdas.
    /// </summary>
    /// <param name="setup">
    ///     The delegate passed to the <c>ConfigureTasks</c> or <c>ConfigureTask</c> methods.
    /// </param>
    internal void SetDeferredSetup(Action<TaskConfig> setup)
    {
        if (setup is null)
            throw new ArgumentNullException(nameof(setup));
        _deferredSetups ??= new List<Action<TaskConfig>>();
        _deferredSetups.Add(setup);
    }

    /// <summary>
    ///     Executes any deferred delegates set up from the .cake file.
    /// </summary>
    internal void PerformDeferredSetup()
    {
        if (_deferredSetups is not null)
        {
            foreach (Action<TaskConfig> deferredSetup in _deferredSetups)
                deferredSetup(this);
        }
    }
}
