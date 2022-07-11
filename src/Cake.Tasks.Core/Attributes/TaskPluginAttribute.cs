// Cake Tasks framework for Cake Build
// Copyright (c) 2019-2022 Jeevan James
// This file is licensed to you under the Apache License, Version 2.0.
// See the LICENSE file in the project root for more information.

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
