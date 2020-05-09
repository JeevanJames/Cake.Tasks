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
