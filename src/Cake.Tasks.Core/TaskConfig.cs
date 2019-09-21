using System;
using System.Collections.Generic;
using System.Text;

namespace Cake.Tasks.Core
{
    public sealed class TaskConfig
    {
        public IDictionary<string, TaskConfigValue> Data { get; } =
            new Dictionary<string, TaskConfigValue>(StringComparer.OrdinalIgnoreCase);
    }

    public sealed class TaskConfigValue
    {
        private readonly object _value;
        private readonly Func<object> _func;

        public TaskConfigValue(object value)
        {
            _value = value;
        }

        public TaskConfigValue(Func<object> func)
        {
            if (func is null)
                throw new ArgumentNullException(nameof(func));
            _func = func;
        }

        public T Resolve<T>()
        {
            if (_func != null)
                return (T)_func();
            return (T)_value;
        }
    }
}
