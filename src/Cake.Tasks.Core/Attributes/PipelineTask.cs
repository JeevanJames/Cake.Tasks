// Cake Tasks framework for Cake Build
// Copyright (c) 2019-2022 Jeevan James
// This file is licensed to you under the Apache License, Version 2.0.
// See the LICENSE file in the project root for more information.

namespace Cake.Tasks.Core;

public enum PipelineTask
{
    /// <summary>
    ///     Code compilation, including code quality analysis.
    /// </summary>
    Build,

    /// <summary>
    ///     Unit testing or fast integration tests.
    /// </summary>
    Test,

    /// <summary>
    ///     Create and build packages, such as NuGet, NPM, Docker images, etc.
    /// </summary>
    Package,

    /// <summary>
    ///     Deploy application.
    /// </summary>
    Deploy,

    /// <summary>
    ///     Full integration test suite; can take time.
    /// </summary>
    IntegrationTest,
}
