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

namespace Cake.Tasks.Core
{
    /// <summary>
    ///     Registers a class in the assembly that has task plugins.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class TaskPluginAttribute : Attribute
    {
        public TaskPluginAttribute(Type pluginType)
        {
            if (pluginType is null)
                throw new ArgumentNullException(nameof(pluginType));
            if (!pluginType.IsClass)
                throw new ArgumentException($"Cake Tasks plugin type {pluginType.FullName} should be a class.", nameof(pluginType));
            PluginType = pluginType;
        }

        public Type PluginType { get; }
    }
}
