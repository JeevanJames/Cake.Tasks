using System;
using System.Collections.Generic;
using System.Text;

namespace Cake.Tasks.Core
{
    public sealed class TaskConfig
    {
        public IDictionary<string, object> Data { get; } = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
    }
}
