// Cake Tasks framework for Cake Build
// Copyright (c) 2019-2022 Jeevan James
// This file is licensed to you under the Apache License, Version 2.0.
// See the LICENSE file in the project root for more information.

namespace Cake.Tasks.Core;

/// <summary>
///     Marks a method as a configuration task.
/// </summary>
public sealed class ConfigAttribute : BaseTaskAttribute
{
    public int Order { get; set; }
}
