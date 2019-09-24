using System;

namespace Cake.Tasks.Core
{
    public sealed class FuncOrValue<T>
    {
        private readonly T _value;
        private readonly Func<T> _func;

        public FuncOrValue(T value)
        {
            _value = value;
        }

        public FuncOrValue(Func<T> func)
        {
            if (func is null)
                throw new ArgumentNullException(nameof(func));
            _func = func;
        }

        public T Resolve()
        {
            return _func != null ? _func() : _value;
        }

        public override string ToString()
        {
            T value = Resolve();
            return value?.ToString() ?? "[NULL]";
        }
    }
}
