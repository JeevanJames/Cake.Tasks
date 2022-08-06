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

        IEnumerable<TaskPluginAttribute> taskPlugins = assembly.GetCustomAttributes<TaskPluginAttribute>();

        foreach (TaskPluginAttribute taskPlugin in taskPlugins)
        {
            Type taskPluginType = taskPlugin.PluginType;
            Log.Verbose($"[Plugin Class] {taskPluginType.FullName}");

            // Get all public static methods from the task plugin type.
            MethodInfo[] methods = taskPluginType.GetMethods(BindingFlags.Static | BindingFlags.Public);
            foreach (MethodInfo method in methods)
            {
                // Gets any task attributes on the method.
                IEnumerable<BaseTaskAttribute> taskAttributes = method.GetCustomAttributes<BaseTaskAttribute>(
                    inherit: true);
                if (!taskAttributes.Any())
                    continue;

                // Method signature should match a valid task method.
                if (!IsValidPluginMethod(method))
                {
                    Log.Warning($"Method {taskPluginType.FullName}.{method.Name} is decorated with one or more task attributes, but does not have the correct signature, so it will not be considered. A valid task method should be a static or instance method that returns void and accepts a first parameter of type {typeof(ICakeContext).FullName} and an optional second parameter of type {typeof(TaskConfig).FullName}.");
                    continue;
                }

                Log.Verbose($"[Plugin Method] {taskPluginType.FullName}.{method.Name}");

                // Go through each task attribute and create a RegisteredTask object
                foreach (BaseTaskAttribute taskAttribute in taskAttributes)
                {
                    RegisteredTask registeredTask = new()
                    {
                        AttributeType = taskAttribute.GetType(),
                        Method = method,
                        CiSystem = taskAttribute.CiSystem,
                        ContinueOnError = taskAttribute.ContinueOnError,
                    };

                    string envSuffix = taskAttribute.CiSystem is null ? string.Empty : $"-{taskAttribute.CiSystem}";
                    string methodDetails = $"{registeredTask.Method.DeclaringType.FullName}.{registeredTask.Method.Name} ({registeredTask.Method.DeclaringType.Assembly.GetName().Name})";

                    switch (taskAttribute)
                    {
                        case PipelineTaskAttribute attr:
                            registeredTask.CoreTask = attr.PipelineTask;
                            registeredTask.Name = $"_{attr.PipelineTask}-{method.Name}{envSuffix}";
                            registeredTask.Description = $"{attr.PipelineTask} task - {methodDetails}";
                            break;
                        case BeforePipelineTaskAttribute attr:
                            registeredTask.CoreTask = attr.PipelineTask;
                            registeredTask.EventType = TaskEventType.BeforeTask;
                            registeredTask.Name = $"_Before{attr.PipelineTask}-{method.Name}{envSuffix}";
                            registeredTask.Description = $"Before {attr.PipelineTask} task - {methodDetails}";
                            break;
                        case AfterPipelineTaskAttribute attr:
                            registeredTask.CoreTask = attr.PipelineTask;
                            registeredTask.EventType = TaskEventType.AfterTask;
                            registeredTask.Name = $"_After{attr.PipelineTask}-{method.Name}{envSuffix}";
                            registeredTask.Description = $"After {attr.PipelineTask} task - {methodDetails}";
                            break;
                        case ConfigAttribute attr:
                            registeredTask.Name = $"_Config-{method.Name}{envSuffix}";
                            registeredTask.Description = $"Config task - {methodDetails}";
                            registeredTask.Order = attr.Order;
                            break;
                        case TaskAttribute attr:
                            registeredTask.Name = method.Name;
                            registeredTask.Description = $"{attr.Description}{envSuffix} - {methodDetails}";
                            registeredTask.RequiresConfig = attr.RequiresConfig;
                            break;
                    }

                    yield return registeredTask;
                }
            }
        }
    }

    protected bool IsValidPluginMethod(MethodInfo method)
    {
        ParameterInfo[] parameters = method.GetParameters();

        // There can be 0 to 2 parameters
        if (parameters.Length > 2)
            return false;

        // The method should return void.
        if (method.ReturnType != typeof(void))
            return false;

        return parameters.Length switch
        {
            2 => typeof(ICakeContext).IsAssignableFrom(parameters[0].ParameterType)
                && parameters[1].ParameterType == typeof(TaskConfig),
            1 => typeof(ICakeContext).IsAssignableFrom(parameters[0].ParameterType)
                || parameters[0].ParameterType == typeof(TaskConfig),
            _ => true,
        };
    }

    protected abstract Assembly ResolveAssembly(object sender, ResolveEventArgs args);
}
