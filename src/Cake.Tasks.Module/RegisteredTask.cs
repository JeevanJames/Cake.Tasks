﻿// Cake Tasks framework for Cake Build
// Copyright (c) 2019-2022 Jeevan James
// This file is licensed to you under the Apache License, Version 2.0.
// See the LICENSE file in the project root for more information.

using System.Reflection;

using Cake.Tasks.Config;
using Cake.Tasks.Core;

namespace Cake.Tasks.Module;

/// <summary>
///     A task registered from a plugin attribute.
/// </summary>
internal abstract class RegisteredTask
{
    /// <summary>
    ///     Gets or sets the name of the plugin. This is used to name the Cake task.
    /// </summary>
    internal string Name { get; set; }

    /// <summary>
    ///     Gets or sets the description of the plugin. This is used to describe the Cake task.
    /// </summary>
    internal string Description { get; set; }

    /// <summary>
    ///     Gets or sets the method that contains the plugin logic.
    /// </summary>
    internal MethodInfo Method { get; set; }

    /// <summary>
    ///     Gets or sets the name of the CI system under which this plugin can run. A <c>null</c>
    ///     value indicates that the plugin will always run.
    /// </summary>
    internal string CiSystem { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the task should swallow any exceptions and
    ///     continue running.
    /// </summary>
    internal bool ContinueOnError { get; set; }

    /// <summary>
    ///     Gets or sets the order of execution of configuration tasks.
    /// </summary>
    internal int Order { get; set; }
}

internal sealed class RegisteredPipelineTask : RegisteredTask
{
    internal PipelineTask? CoreTask { get; set; }
}

internal sealed class RegisteredBeforeAfterPipelineTask : RegisteredTask
{
    internal PipelineTask? CoreTask { get; set; }

    internal TaskEventType? EventType { get; set; }
}

internal sealed class RegisteredConfigTask : RegisteredTask
{
}

internal sealed class RegisteredTeardownTask : RegisteredTask
{
}

internal sealed class RegisteredRegularTask : RegisteredTask
{
    /// <summary>
    ///     Gets or sets a value indicating whether a regular task (registered with <see cref="TaskAttribute"/>)
    ///     needs the task configuration (<see cref="TaskConfig"/>). If so, then the task adds a dependency
    ///     on the config task.
    /// </summary>
    internal bool RequiresConfig { get; set; }
}
