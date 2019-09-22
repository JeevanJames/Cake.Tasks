using System;
using System.Collections.Generic;

namespace Cake.Tasks.Core
{
    public sealed class TaskConfig
    {
        public IDictionary<string, TaskConfigValue> Data { get; } =
            new Dictionary<string, TaskConfigValue>(StringComparer.OrdinalIgnoreCase);

        public T Resolve<T>(string name, T defaultValue = default)
        {
            if (!Data.TryGetValue(name, out TaskConfigValue value))
                return defaultValue;
            return value.Resolve<T>();
        }
    }
}
