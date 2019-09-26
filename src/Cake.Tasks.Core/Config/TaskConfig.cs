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
