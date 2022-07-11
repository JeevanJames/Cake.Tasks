// Cake Tasks framework for Cake Build
// Copyright (c) 2019-2022 Jeevan James
// This file is licensed to you under the Apache License, Version 2.0.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;

namespace Cake.Tasks.Config
{
    public sealed class TaskConfig
    {
        public static TaskConfig Current { get; } = new TaskConfig();

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
            var config = (T)Activator.CreateInstance(typeof(T), this);
            return config;
        }

        internal IDictionary<string, object> Data { get; } =
            new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        private List<Action<TaskConfig>> _deferredSetups;

        internal void SetDeferredSetup(Action<TaskConfig> setup)
        {
            if (_deferredSetups is null)
                _deferredSetups = new List<Action<TaskConfig>>();
            _deferredSetups.Add(setup);
        }

        internal void PerformDeferredSetup()
        {
            if (_deferredSetups != null)
            {
                foreach (Action<TaskConfig> deferredSetup in _deferredSetups)
                    deferredSetup(this);
            }
        }
    }
}
