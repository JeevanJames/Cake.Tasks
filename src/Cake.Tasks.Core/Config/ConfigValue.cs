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

namespace Cake.Tasks.Config
{
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

        public T Resolve()
        {
            return _func != null ? _func() : _value;
        }

        public override string ToString()
        {
            T value = Resolve();
            return value?.ToString() ?? "[NULL]";
        }

        public static implicit operator ConfigValue<T>(T value) => new ConfigValue<T>(value);

        public static implicit operator ConfigValue<T>(Func<T> func) => new ConfigValue<T>(func);

        public static implicit operator T(ConfigValue<T> instance)
        {
            return instance is null ? default : instance.Resolve();
        }
    }
}
