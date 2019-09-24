using System;
using System.Collections.Generic;

namespace Cake.Tasks.Core
{
    public sealed class TaskConfig
    {
        public static TaskConfig Current { get; } = new TaskConfig();

        private TaskConfig()
        {
        }

        internal IDictionary<string, object> Data { get; } =
            new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        //public IList<T> ResolveAsList<T>(string name)
        //{
        //    if (!Data.TryGetValue(name, out TaskConfigValue tcValue))
        //        return Array.Empty<T>();

        //    object value = tcValue.Resolve<object>();

        //    if (value is null)
        //        return Array.Empty<T>();

        //    if (value is T item)
        //        return new T[] { item };

        //    if (value is IEnumerable<T> list)
        //        return new List<T>(list);

        //    throw new InvalidCastException($"Cannot cast the config value {name} to {typeof(T).FullName}. The actual type is {value.GetType().FullName}.");
        //}

        public T Load<T>()
            where T : PluginConfig
        {
            var config = (T)Activator.CreateInstance(typeof(T), this);
            return config;
        }

        private Action<TaskConfig> _setup;

        public void SetDeferredSetup(Action<TaskConfig> setup)
        {
            _setup = setup;
        }

        public void PerformDeferredSetup()
        {
            _setup?.Invoke(this);
        }
    }

    public abstract class PluginConfig
    {
        private readonly TaskConfig _taskConfig;

        protected PluginConfig(TaskConfig taskConfig)
        {
            _taskConfig = taskConfig;
        }

        protected T Get<T>(string name)
        {
            if (!_taskConfig.Data.TryGetValue(name, out object value))
                return default;
            return (T)value;
        }

        protected void Set<T>(string name, T value)
        {
            if (_taskConfig.Data.ContainsKey(name))
                _taskConfig.Data[name] = value;
            else
                _taskConfig.Data.Add(name, value);
        }
    }
}
