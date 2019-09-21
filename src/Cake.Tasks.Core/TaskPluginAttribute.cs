﻿using System;

namespace Cake.Tasks.Core
{
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