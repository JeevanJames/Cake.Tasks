using System;
using System.Collections.Generic;

namespace Cake.Tasks.Core
{
    public sealed class TaskConfig
    {
        public IDictionary<string, TaskConfigValue> Data { get; } =
            new Dictionary<string, TaskConfigValue>(StringComparer.OrdinalIgnoreCase);
    }
}
