// Cake Tasks framework for Cake Build
// Copyright (c) 2019-2022 Jeevan James
// This file is licensed to you under the Apache License, Version 2.0.
// See the LICENSE file in the project root for more information.

using System;

namespace Cake.Tasks.Core;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public abstract class BaseTaskAttribute : Attribute
{
    /// <summary>
    ///     Gets or sets the name of the CI system in which this task can be executed (TFS,
    ///     AppVeyor, etc.).
    ///     <para/>
    ///     If not specified, this task will always be executed.
    /// </summary>
    public string CiSystem { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the task should swallow any exceptions and
    ///     continue running.
    /// </summary>
    public bool ContinueOnError { get; set; }
}
