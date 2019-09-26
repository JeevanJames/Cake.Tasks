using System;
using System.ComponentModel;

namespace Cake.Tasks.Config
{
    public sealed class FuncOrValue<T> : IStringParsable
    {
        private readonly T _value;
        private readonly Func<T> _func;

        private FuncOrValue(T value)
        {
            _value = value;
        }

        private FuncOrValue(Func<T> func)
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

        public object FromString(string str)
        {
            TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
            if (!converter.CanConvertFrom(typeof(string)))
                return default;
            return converter.ConvertFromString(str);
        }

        public static implicit operator FuncOrValue<T>(T value) => new FuncOrValue<T>(value);

        public static implicit operator FuncOrValue<T>(Func<T> func) => new FuncOrValue<T>(func);

        public static implicit operator T(FuncOrValue<T> instance)
        {
            return instance is null ? default : instance.Resolve();
        }
    }

    public interface IStringParsable
    {
        object FromString(string str);
    }
}
