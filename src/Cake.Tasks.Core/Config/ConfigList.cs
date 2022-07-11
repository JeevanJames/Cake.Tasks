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

// ReSharper disable once CheckNamespace
namespace Cake.Tasks.Config
{
    /// <summary>
    ///     Represents a configuration list of values as either a static value, a static list of values
    ///     or a factory delegate that can instantiate the list.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
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
            _valueFunc = valueFunc ?? throw new ArgumentNullException(nameof(valueFunc));
        }

        private ConfigList(Func<IList<T>> listFunc)
        {
            _listFunc = listFunc ?? throw new ArgumentNullException(nameof(listFunc));
        }

        /// <summary>
        ///     Resolves the configuration list from either its fixed value, or from the factory delegate,
        ///     depending on how it was setup.
        /// </summary>
        /// <returns>The final value of the configuration list.</returns>
        public IList<T> Resolve()
        {
            if (_listFunc is not null)
                return _listFunc();
            if (_list is not null)
                return _list;
            T value = _valueFunc is not null ? _valueFunc() : _value;
            return new[] { value };
        }

        public override string ToString()
        {
            IList<T> values = Resolve();
            if (values is null || values.Count == 0)
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

        public static implicit operator ConfigList<T>(T value) => new(value);

        public static implicit operator ConfigList<T>(Func<T> valueFunc) => new(valueFunc);

        public static implicit operator ConfigList<T>(T[] list) => new(list);

        public static implicit operator ConfigList<T>(List<T> list) => new(list);

        public static implicit operator ConfigList<T>(Func<IList<T>> listFunc) => new(listFunc);

        public static implicit operator List<T>(ConfigList<T> instance) => instance.Resolve().ToList();
    }
}
