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
    public sealed class FuncOrValue<T>
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

        public static implicit operator FuncOrValue<T>(T value) => new FuncOrValue<T>(value);

        public static implicit operator FuncOrValue<T>(Func<T> func) => new FuncOrValue<T>(func);

        public static implicit operator T(FuncOrValue<T> instance)
        {
            return instance is null ? default : instance.Resolve();
        }
    }
}
