// Cake Tasks framework for Cake Build
// Copyright (c) 2019-2022 Jeevan James
// This file is licensed to you under the Apache License, Version 2.0.
// See the LICENSE file in the project root for more information.

using Cake.Core;
using Cake.Core.Annotations;
using Cake.Core.Composition;
using Cake.Tasks.Module;

[assembly: CakeModule(typeof(TasksModule))]

namespace Cake.Tasks.Module;

public sealed class TasksModule : ICakeModule
{
    public void Register(ICakeContainerRegistrar registrar)
    {
        registrar.RegisterType<TasksEngine>().As<ICakeEngine>().Singleton();
    }
}
