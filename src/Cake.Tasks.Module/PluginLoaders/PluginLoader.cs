#region --- License & Copyright Notice ---
/*
Cake Tasks
Copyright 2019 Jeevan James

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/
#endregion

#pragma warning disable S3885 // "Assembly.Load" should be used

using System;
using System.Collections.Generic;
using System.Reflection;

using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Tasks.Config;
using Cake.Tasks.Core;

namespace Cake.Tasks.Module.PluginLoaders
{
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
            var assembly = Assembly.LoadFile(dllFile);

            IEnumerable<TaskPluginAttribute> taskPlugins = assembly.GetCustomAttributes<TaskPluginAttribute>();

            foreach (TaskPluginAttribute taskPlugin in taskPlugins)
            {
                Type taskPluginType = taskPlugin.PluginType;
                Log.Verbose($"[Plugin Class] {taskPluginType.FullName}");
                MethodInfo[] methods = taskPluginType.GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public);
                foreach (MethodInfo method in methods)
                {
                    BaseTaskAttribute taskAttribute = method.GetCustomAttribute<BaseTaskAttribute>(inherit: true);
                    if (taskAttribute is null)
                        continue;
                    if (!IsValidPluginMethod(method))
                    {
                        Log.Warning($"Method {taskPluginType.FullName}.{method.Name} is decorated with a task attribute ({taskAttribute.GetType().Name}), but does not have the correct signature, so it will not be considered. A valid task method should be a static or instance method that returns void and accepts a first parameter of type {typeof(ICakeContext).FullName} and an optional second parameter of type {typeof(TaskConfig).FullName}.");
                        continue;
                    }

                    Log.Verbose($"[Plugin Method] {taskPluginType.FullName}.{method.Name}");

                    var registeredTask = new RegisteredTask
                    {
                        AttributeType = taskAttribute.GetType(),
                        Method = method,
                        CiSystem = taskAttribute.CiSystem,
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
                        case TaskEventAttribute attr:
                            registeredTask.CoreTask = attr.PipelineTask;
                            registeredTask.EventType = attr.EventType;
                            string namePrefix = attr.EventType == TaskEventType.BeforeTask ? "Before" : "After";
                            registeredTask.Name = $"_{namePrefix}{attr.PipelineTask}-{method.Name}{envSuffix}";
                            registeredTask.Description = $"{namePrefix} {attr.PipelineTask} task - {methodDetails}";
                            break;
                        case ConfigAttribute attr:
                            registeredTask.Name = $"_Config-{method.Name}{envSuffix}";
                            registeredTask.Description = $"Config task - {methodDetails}";
                            registeredTask.Order = attr.Order;
                            break;
                        case TaskAttribute attr:
                            registeredTask.Name = method.Name;
                            registeredTask.Description = $"{attr.Description}{envSuffix} - {methodDetails}";
                            break;
                    }

                    yield return registeredTask;
                }
            }
        }

        protected bool IsValidPluginMethod(MethodInfo method)
        {
            ParameterInfo[] parameters = method.GetParameters();

            // There can be one or two parameters.
            if (parameters.Length < 1 || parameters.Length > 2)
                return false;

            // The first parameter should be ICakeContext.
            if (!typeof(ICakeContext).IsAssignableFrom(parameters[0].ParameterType))
                return false;

            // The second parameter, if specified, should be TaskConfig.
            if (parameters.Length > 1 && parameters[1].ParameterType != typeof(TaskConfig))
                return false;

            // The method should return void.
            if (method.ReturnType != typeof(void))
                return false;

            return true;
        }

        protected abstract Assembly ResolveAssembly(object sender, ResolveEventArgs args);
    }
}
