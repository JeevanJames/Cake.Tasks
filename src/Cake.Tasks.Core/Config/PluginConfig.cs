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
using System.ComponentModel;
using System.Reflection;
using Cake.Tasks.Core;

namespace Cake.Tasks.Config
{
    public abstract class PluginConfig
    {
        private readonly TaskConfig _taskConfig;

        protected PluginConfig(TaskConfig taskConfig)
        {
            _taskConfig = taskConfig;
        }

        protected T Get<T>(string name)
        {
            if (!_taskConfig.Data.TryGetValue(name, out object value))
                return default;
            if (value is null)
                return default;
            if (value is string str && typeof(T) != typeof(string))
            {
                if (TryFromFuncOrValue<T>(str, out T fovValue))
                    return fovValue;
                return (T)FromString(str, typeof(T));
            }

            return (T)value;
        }

        protected FuncOrListValue<T> GetFuncOrListValue<T>(string name) =>
            Get<FuncOrListValue<T>>(name);

        protected T GetFuncOrValue<T>(string name) =>
            Get<FuncOrValue<T>>(name);

        private bool TryFromFuncOrValue<T>(string str, out T value)
        {
            value = default;
            Type type = typeof(T);
            if (!type.IsGenericType)
                return false;
            Type genericTypeDef = type.GetGenericTypeDefinition();
            if (genericTypeDef != typeof(FuncOrValue<>))
                return false;

            Type dataType = type.GetGenericArguments()[0];
            object data = FromString(str, dataType);

            value = (T)Activator.CreateInstance(type, BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { data }, null);
            return true;
        }

        private static object FromString(string str, Type type)
        {
            TypeConverter converter = TypeDescriptor.GetConverter(type);
            if (!converter.CanConvertFrom(typeof(string)))
                return default;
            return converter.ConvertFromString(str);
        }

        protected void Set<T>(string name, T value)
        {
            if (_taskConfig.Data.ContainsKey(name))
                _taskConfig.Data[name] = value;
            else
                _taskConfig.Data.Add(name, value);
        }
    }
}
