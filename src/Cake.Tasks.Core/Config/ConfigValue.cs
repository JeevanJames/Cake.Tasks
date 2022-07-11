// Cake Tasks framework for Cake Build
// Copyright (c) 2019-2022 Jeevan James
// This file is licensed to you under the Apache License, Version 2.0.
// See the LICENSE file in the project root for more information.

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
