using System;

namespace Cake.Tasks.Core
{
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
            return _func != null ? (T)_func() : (T)_value;
        }

        public static implicit operator TaskConfigValue(Func<object> func) => new TaskConfigValue(func);

        public static implicit operator TaskConfigValue(string value) => new TaskConfigValue(value);

        public static implicit operator TaskConfigValue(int value) => new TaskConfigValue(value);

        public static implicit operator TaskConfigValue(bool value) => new TaskConfigValue(value);

        public static implicit operator TaskConfigValue(Uri value) => new TaskConfigValue(value);
    }
}
