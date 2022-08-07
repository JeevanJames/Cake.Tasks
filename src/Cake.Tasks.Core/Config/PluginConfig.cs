// Cake Tasks framework for Cake Build
// Copyright (c) 2019-2022 Jeevan James
// This file is licensed to you under the Apache License, Version 2.0.
// See the LICENSE file in the project root for more information.

using System;
using System.ComponentModel;
using System.Reflection;

using Cake.Tasks.Core;

// ReSharper disable once CheckNamespace
namespace Cake.Tasks.Config;

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
        if (taskConfig is null)
            throw new ArgumentNullException(nameof(taskConfig));
        _taskConfig = taskConfig;
    }

    protected T Get<T>(string name, T defaultValue = default)
    {
        // If the entry is not in the dictionary, add the default value as the entry and return it.
        if (!_taskConfig.Data.TryGetValue(name, out object value))
        {
            _taskConfig.Data.Add(name, defaultValue);
            return defaultValue;
        }

        // If the entry is in the dictionary, but the value is null, return the default value.
        if (value is null)
            return defaultValue;

        // If the value's type is the same as the property type (T), then just cast and return.
        if (typeof(T) == value.GetType())
            return (T)value;

        // If the property type (T) is a ConfigValue<>, but the underlying dictionary value is equal
        // to the ConfigValue's generic type, it means that someone set the dictionary value directly
        // to a value of the ConfigValue's generic type. In this case, we need to use reflection to
        // create a new ConfigValue<> instance with the dictionary value as its underlying value.
        if (TryReadAsConfigValue(value, out T configValue))
            return configValue;

        //TODO: Should we read ConfigList<> values also?

        if (value is string str && typeof(T) != typeof(string))
        {
            if (TryFromConfigValue(str, out T fovValue))
                return fovValue;
            return (T)FromString(str, typeof(T));
        }

        throw new TaskConfigException($"The config value named {name} cannot be returned as a {typeof(T)} type.");
    }

    protected ConfigList<T> GetList<T>(string name)
    {
        return Get<ConfigList<T>>(name);
    }

    protected T GetValue<T>(string name)
    {
        return Get<ConfigValue<T>>(name);
    }

    private static bool TryFromConfigValue<T>(string str, out T value)
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

#pragma warning disable S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields
        value = (T)Activator.CreateInstance(type, BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { data }, null);
#pragma warning restore S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields
        return true;
    }

    private static bool TryReadAsConfigValue<T>(object rawValue, out T value)
    {
        if (rawValue is null || !IsConfigValue<T>(out Type underlyingType))
        {
            value = default;
            return false;
        }

        if (underlyingType == rawValue.GetType())
        {
            Type configValueType = typeof(ConfigValue<>).MakeGenericType(underlyingType);
#pragma warning disable S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields
            value = (T)Activator.CreateInstance(configValueType, BindingFlags.NonPublic | BindingFlags.Instance,
                binder: null,
                args: new[] { rawValue },
                culture: null);
#pragma warning restore S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields
            return true;
        }

        value = default;
        return false;
    }

    private static bool IsConfigValue<T>(out Type underlyingType)
    {
        underlyingType = null;

        Type type = typeof(T);
        if (!type.IsGenericType)
            return false;

        Type genericTypeDef = type.GetGenericTypeDefinition();
        if (genericTypeDef != typeof(ConfigValue<>))
            return false;

        underlyingType = type.GetGenericArguments()[0];
        return true;
    }

    private static object FromString(string str, Type type)
    {
        TypeConverter converter = TypeDescriptor.GetConverter(type);
        return converter.CanConvertFrom(typeof(string)) ? converter.ConvertFromString(str) : default;
    }

    protected void Set<T>(string name, T value)
    {
        if (_taskConfig.Data.ContainsKey(name))
            _taskConfig.Data[name] = value;
        else
            _taskConfig.Data.Add(name, value);
    }
}
