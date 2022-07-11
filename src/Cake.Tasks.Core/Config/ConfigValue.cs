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

// ReSharper disable once CheckNamespace
namespace Cake.Tasks.Config
{
    /// <summary>
    ///     Represents a configuration value as either a static value or as a factory delegate that
    ///     can instantiate the value.
    /// </summary>
    /// <typeparam name="T">The type of the configuration value.</typeparam>
    public sealed class ConfigValue<T>
    {
        private readonly T _value;
        private readonly Func<T> _func;

        private ConfigValue(T value)
        {
            _value = value;
        }

        private ConfigValue(Func<T> func)
        {
            if (func is null)
                throw new ArgumentNullException(nameof(func));
            _func = func;
        }

        /// <summary>
        ///     Resolves the value of the configuration from either its fixed value, or from the factory
        ///     delegate, depending on how it was setup.
        /// </summary>
        /// <returns>The final value of the configuration value.</returns>
        public T Resolve()
        {
            return _func is not null ? _func() : _value;
        }

        public override string ToString()
        {
            T value = Resolve();
            return value?.ToString() ?? "[NULL]";
        }

        public static implicit operator ConfigValue<T>(T value) => new(value);

        public static implicit operator ConfigValue<T>(Func<T> func) => new(func);

        public static implicit operator T(ConfigValue<T> instance)
        {
            return instance is null ? default : instance.Resolve();
        }
    }
}
