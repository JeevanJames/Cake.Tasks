using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cake.Tasks.Core
{
    public sealed class FuncOrListValue<T>
    {
        private readonly T _value;
        private readonly IEnumerable<T> _list;
        private readonly Func<T> _valueFunc;
        private readonly Func<IEnumerable<T>> _listFunc;

        public FuncOrListValue(T value)
        {
            _value = value;
        }

        public FuncOrListValue(IEnumerable<T> list)
        {
            _list = list;
        }

        public FuncOrListValue(Func<T> valueFunc)
        {
            if (valueFunc is null)
                throw new ArgumentNullException(nameof(valueFunc));
            _valueFunc = valueFunc;
        }

        public FuncOrListValue(Func<IEnumerable<T>> listFunc)
        {
            if (listFunc is null)
                throw new ArgumentNullException(nameof(listFunc));
            _listFunc = listFunc;
        }

        public IEnumerable<T> Resolve()
        {
            if (_listFunc != null)
                return _listFunc();
            if (_list != null)
                return _list;
            T value = _valueFunc != null ? _valueFunc() : _value;
            return new T[] { value };
        }

        public override string ToString()
        {
            IEnumerable<T> values = Resolve();
            if (values is null)
                return "[]";
            return values.Aggregate(new StringBuilder(),
                (sb, value) =>
                {
                    if (sb.Length > 0)
                        sb.Append(Environment.NewLine);
                    sb.Append(value?.ToString() ?? "[NULL]");
                    return sb;
                }).ToString();
        }
    }
}
