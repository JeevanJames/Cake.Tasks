﻿// Cake Tasks framework for Cake Build
// Copyright (c) 2019-2022 Jeevan James
// This file is licensed to you under the Apache License, Version 2.0.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;

// ReSharper disable once CheckNamespace
namespace Cake.Tasks.Config;

/// <summary>
///     Represents an item that can be published or deployed from the current build.
/// </summary>
public abstract class Publisher
{
    /// <summary>
    ///     Additional properties that can be set on the publisher to control the behavior of
    ///     specific tasks.
    /// </summary>
    private readonly Dictionary<string, object> _properties = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///     Indicates whether the output properties - <see cref="OutputLocation"/> and
    ///     <see cref="OutputType"/> have been set using the <see cref="SetOutput"/> method. This
    ///     is used to ensure that the method is called only once.
    /// </summary>
    private bool _isOutputSet;

    private PublishOutputType _outputType;

    /// <summary>
    ///     Gets the directory where the published output is copied to, or the file name of the
    ///     published output, depending on the value of <see cref="OutputType"/>.
    /// </summary>
    public string OutputLocation { get; private set; }

    /// <summary>
    ///     Gets whether the published output is a directory of multiple files or a single file.
    /// </summary>
    public virtual PublishOutputType OutputType => _outputType;

    public void SetOutput(string location, PublishOutputType type = PublishOutputType.Directory)
    {
        if (_isOutputSet)
            throw new InvalidOperationException("The output is already set for this publisher. Cannot set it again.");

        if (type == PublishOutputType.Directory && !Directory.Exists(location))
            throw new DirectoryNotFoundException($"The specified output directory '{location ?? string.Empty}' does not exist.");
        if (type == PublishOutputType.File && !File.Exists(location))
            throw new FileNotFoundException("The specified output file does not exist.", location ?? string.Empty);

        OutputLocation = location;
        _outputType = type;

        _isOutputSet = true;
    }

    public T GetProperty<T>(string name)
    {
        return _properties.TryGetValue(name, out object obj) ? (T)obj : default;
    }

    public Publisher SetProperty<T>(string name, T value)
    {
        _properties.Add(name, value);
        return this;
    }
}

public enum PublishOutputType
{
    Directory,
    File,
}
