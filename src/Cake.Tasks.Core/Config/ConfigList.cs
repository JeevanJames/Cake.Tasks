#region --- License & Copyright Notice ---
/*
Cake Tasks
Copyright 2019 Jeevan James

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cake.Tasks.Config
{
    public sealed class ConfigList<T>
    {
        private readonly T _value;
        private readonly IList<T> _list;
        private readonly Func<T> _valueFunc;
        private readonly Func<IList<T>> _listFunc;

        private ConfigList(T value)
        {
            _value = value;
        }

        private ConfigList(IList<T> list)
        {
            _list = list;
        }

        private ConfigList(Func<T> valueFunc)
        {
            if (valueFunc is null)
                throw new ArgumentNullException(nameof(valueFunc));
            _valueFunc = valueFunc;
        }

        private ConfigList(Func<IList<T>> listFunc)
        {
            if (listFunc is null)
                throw new ArgumentNullException(nameof(listFunc));
            _listFunc = listFunc;
        }

        public IList<T> Resolve()
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
            IList<T> values = Resolve();
            if (values is null || !values.Any())
                return "[]";
            StringBuilder builder = values.Aggregate(new StringBuilder("["),
                (sb, value) =>
                {
                    if (sb.Length > 0)
                        sb.AppendLine();
                    sb.Append(value?.ToString() ?? "[NULL]");
                    return sb;
                });
            builder.AppendLine().Append("]");
            return builder.ToString();
        }

        public static implicit operator ConfigList<T>(T value) => new ConfigList<T>(value);

        public static implicit operator ConfigList<T>(Func<T> valueFunc) => new ConfigList<T>(valueFunc);

        public static implicit operator ConfigList<T>(T[] list) => new ConfigList<T>(list);

        public static implicit operator ConfigList<T>(List<T> list) => new ConfigList<T>(list);

        public static implicit operator ConfigList<T>(Func<IList<T>> listFunc) => new ConfigList<T>(listFunc);

        public static implicit operator List<T>(ConfigList<T> instance) => instance.Resolve().ToList();
    }
}
