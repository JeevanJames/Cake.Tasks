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
using System.ComponentModel;
using System.Reflection;

namespace Cake.Tasks.Config
{
    /// <summary>
    ///     Represents the config object for a Cake.Task plugin.
    ///     <para/>
    ///     Plugins should create a class that derives from this class, if they want to have
    ///     configuration.
    /// </summary>
    /// <example><see cref="EnvConfig"/>.</example>
    public abstract class PluginConfig
    {
        private readonly TaskConfig _taskConfig;

        protected PluginConfig(TaskConfig taskConfig)
        {
            _taskConfig = taskConfig;
        }

        protected T Get<T>(string name, T defaultValue = default)
        {
            if (!_taskConfig.Data.TryGetValue(name, out object value))
            {
                _taskConfig.Data.Add(name, defaultValue);
                return defaultValue;
            }

            if (value is null)
                return defaultValue;
            if (value is string str && typeof(T) != typeof(string))
            {
                if (TryFromConfigValue<T>(str, out T fovValue))
                    return fovValue;
                return (T)FromString(str, typeof(T));
            }

            return (T)value;
        }

        protected ConfigList<T> GetList<T>(string name) =>
            Get<ConfigList<T>>(name);

        protected T GetValue<T>(string name) =>
            Get<ConfigValue<T>>(name);

        private bool TryFromConfigValue<T>(string str, out T value)
        {
            value = default;
            Type type = typeof(T);
            if (!type.IsGenericType)
                return false;
            Type genericTypeDef = type.GetGenericTypeDefinition();
            if (genericTypeDef != typeof(ConfigValue<>))
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
