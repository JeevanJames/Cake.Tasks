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

        public IDictionary<string, TaskConfigValue> Data { get; } =
            new Dictionary<string, TaskConfigValue>(StringComparer.OrdinalIgnoreCase);

        public void Register(string name, TaskConfigValue value)
        {
            Data.Add(name, value);
        }

        public void Set(string name, TaskConfigValue value)
        {
            if (Data.ContainsKey(name))
                Data[name] = value;
        }

        public T Resolve<T>(string name, T defaultValue = default)
        {
            if (!Data.TryGetValue(name, out TaskConfigValue value))
                return defaultValue;
            return value.Resolve<T>();
        }

        public IList<T> ResolveAsList<T>(string name)
        {
            if (!Data.TryGetValue(name, out TaskConfigValue tcValue))
                return Array.Empty<T>();

            object value = tcValue.Resolve<object>();

            if (value is null)
                return Array.Empty<T>();

            if (value is T item)
                return new T[] { item };

            if (value is IEnumerable<T> list)
                return new List<T>(list);

            throw new InvalidCastException($"Cannot cast the config value {name} to {typeof(T).FullName}. The actual type is {value.GetType().FullName}.");
        }

        public bool TryResolve<T>(string name, out T value)
        {
            bool found = Data.TryGetValue(name, out TaskConfigValue tcValue);
            value = found ? tcValue.Resolve<T>() : default;
            return found;
        }
    }
}
