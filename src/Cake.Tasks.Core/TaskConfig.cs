using System;
using System.Collections.Generic;

namespace Cake.Tasks.Core
{
    public sealed class TaskConfig
    {
        public IDictionary<string, TaskConfigValue> Data { get; } =
            new Dictionary<string, TaskConfigValue>(StringComparer.OrdinalIgnoreCase);

        public void Register(string name, TaskConfigValue value)
        {
            Data.Add(name, value);
        }

        public T Resolve<T>(string name, T defaultValue = default)
        {
            if (!Data.TryGetValue(name, out TaskConfigValue value))
                return defaultValue;
            return value.Resolve<T>();
        }

        public bool TryResolve<T>(string name, out T value)
        {
            bool found = Data.TryGetValue(name, out TaskConfigValue tcValue);
            value = found ? tcValue.Resolve<T>() : default;
            return found;
        }
    }
}
