// Cake Tasks framework for Cake Build
// Copyright (c) 2019-2022 Jeevan James
// This file is licensed to you under the Apache License, Version 2.0.
// See the LICENSE file in the project root for more information.

#pragma warning disable S3885 // "Assembly.Load" should be used

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Tasks.Config;
using Cake.Tasks.Core;

namespace Cake.Tasks.Module.PluginLoaders;

/// <summary>
///     Encapsulates the logic to load Cake.Tasks plugins.
/// </summary>
public abstract class PluginLoader
{
    protected PluginLoader(string pluginsDir, ICakeLog log)
    {
        PluginsDir = pluginsDir;
        Log = log;
    }

    internal abstract IEnumerable<RegisteredTask> LoadPlugins();

    protected string PluginsDir { get; }

    protected ICakeLog Log { get; }

    internal IEnumerable<RegisteredTask> FindPlugins(string dllFile)
    {
        Assembly assembly = Assembly.LoadFile(dllFile);

        foreach (TaskPluginAttribute taskPlugin in assembly.GetCustomAttributes<TaskPluginAttribute>())
        {
            Type taskPluginType = taskPlugin.PluginType;
            Log.Verbose($"[Plugin Class] {taskPluginType.FullName}");

            // Get all public static methods from the task plugin type.
            MethodInfo[] methods = taskPluginType.GetMethods(BindingFlags.Static | BindingFlags.Public);
            foreach (MethodInfo method in methods)
            {
                // Gets any task attributes on the method.
                IEnumerable<BaseTaskAttribute> taskAttributes = method
                    .GetCustomAttributes<BaseTaskAttribute>(inherit: true);
                if (!taskAttributes.Any())
                    continue;

                // Method signature should match a valid task method.
                if (!IsValidPluginMethod(method))
                {
                    string message = string.Join(" ",
                        $"Method {taskPluginType.FullName}.{method.Name} is decorated with one or more task attributes, but does not have the correct signature, so it will not be considered.",
                        $"A valid task method should be a static or instance method that returns void and accepts a first parameter of type {typeof(ICakeContext).FullName} and an optional second parameter of type {typeof(TaskConfig).FullName}.");
                    Log.Warning(message);
                    continue;
                }

                if (method.DeclaringType is null)
                    throw new InvalidOperationException($"Task plugin method {method.Name} does not have a declaring type.");

                Log.Verbose($"[Plugin Method] {taskPluginType.FullName}.{method.Name}");

                // Go through each task attribute and create a RegisteredTask object
                foreach (BaseTaskAttribute taskAttribute in taskAttributes)
                {
                    string envSuffix = taskAttribute.CiSystem is null ? string.Empty : $"-{taskAttribute.CiSystem}";
                    string methodDetails = $"{method.DeclaringType.FullName}.{method.Name} ({method.DeclaringType.Assembly.GetName().Name})";

                    RegisteredTask registeredTask = taskAttribute switch
                    {
                        PipelineTaskAttribute attr => new RegisteredPipelineTask
                        {
                            Name = $"_{attr.PipelineTask}-{method.Name}{envSuffix}",
                            Description = $"{attr.PipelineTask} task - {methodDetails}",
                            CoreTask = attr.PipelineTask,
                        },
                        BeforePipelineTaskAttribute attr => new RegisteredBeforeAfterPipelineTask
                        {
                            Name = $"_Before{attr.PipelineTask}-{attr.Order}-{method.Name}{envSuffix}",
                            Description = $"Before {attr.PipelineTask} task - {methodDetails}",
                            CoreTask = attr.PipelineTask,
                            EventType = TaskEventType.BeforeTask,
                            Order = attr.Order,
                        },
                        AfterPipelineTaskAttribute attr => new RegisteredBeforeAfterPipelineTask
                        {
                            Name = $"_After{attr.PipelineTask}-{attr.Order}-{method.Name}{envSuffix}",
                            Description = $"After {attr.PipelineTask} task - {methodDetails}",
                            CoreTask = attr.PipelineTask,
                            EventType = TaskEventType.AfterTask,
                            Order = attr.Order,
                        },
                        TeardownTaskAttribute attr => new RegisteredTeardownTask
                        {
                            Name = $"_Teardown-{attr.Order}-{method.Name}{envSuffix}",
                            Description = $"Teardown task - {methodDetails}",
                            Order = attr.Order,
                        },
                        ConfigAttribute attr => new RegisteredConfigTask
                        {
                            Name = $"_Config-{attr.Order}-{method.Name}{envSuffix}",
                            Description = $"Config task - {methodDetails}",
                            Order = attr.Order,
                        },
                        TaskAttribute attr => new RegisteredRegularTask
                        {
                            Name = method.Name,
                            Description = $"{attr.Description}{envSuffix} - {methodDetails}",
                            RequiresConfig = attr.RequiresConfig,
                        },
                        _ => throw new NotSupportedException($"Unknown task attribute - {taskAttribute.GetType()}."),
                    };

                    registeredTask.Method = method;
                    registeredTask.CiSystem = taskAttribute.CiSystem;
                    registeredTask.ContinueOnError = taskAttribute.ContinueOnError;

                    yield return registeredTask;
                }
            }
        }
    }

    protected bool IsValidPluginMethod(MethodInfo method)
    {
        // The method should return void.
        if (method.ReturnType != typeof(void))
            return false;

        ParameterInfo[] parameters = method.GetParameters();

        // The method should have zero to two parameters.
        return parameters.Length switch
        {
            2 => typeof(ICakeContext).IsAssignableFrom(parameters[0].ParameterType)
                && parameters[1].ParameterType == typeof(TaskConfig),
            1 => typeof(ICakeContext).IsAssignableFrom(parameters[0].ParameterType)
                || parameters[0].ParameterType == typeof(TaskConfig),
            0 => true,
            _ => false,
        };
    }

    protected abstract Assembly ResolveAssembly(object sender, ResolveEventArgs args);
}
