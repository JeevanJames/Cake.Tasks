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
                    BasePipelineTaskAttribute taskAttribute = method.GetCustomAttribute<BasePipelineTaskAttribute>(inherit: true);
                    if (taskAttribute is null)
                        continue;
                    if (!IsValidPluginMethod(method, taskAttribute))
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

                    switch (taskAttribute)
                    {
                        case PipelineTaskAttribute attr:
                            registeredTask.CoreTask = attr.PipelineTask;
                            registeredTask.Name = $"_{attr.PipelineTask}-{method.Name}{envSuffix}";
                            break;
                        case TaskEventAttribute attr:
                            registeredTask.CoreTask = attr.CoreTask;
                            registeredTask.EventType = attr.EventType;
                            string namePrefix = attr.EventType == TaskEventType.BeforeTask ? "Before" : "After";
                            registeredTask.Name = $"_{namePrefix}{attr.CoreTask}-{method.Name}{envSuffix}";
                            break;
                        case ConfigAttribute attr:
                            registeredTask.Name = $"_Config-{method.Name}{envSuffix}";
                            registeredTask.Order = attr.Order;
                            break;
                    }

                    yield return registeredTask;
                }
            }
        }

        protected bool IsValidPluginMethod(MethodInfo method, BasePipelineTaskAttribute attribute)
        {
            ParameterInfo[] parameters = method.GetParameters();
            switch (attribute)
            {
                case PipelineTaskAttribute _:
                case TaskEventAttribute _:
                    if (parameters.Length < 1 || parameters.Length > 2)
                        return false;
                    if (!typeof(ICakeContext).IsAssignableFrom(parameters[0].ParameterType))
                        return false;
                    if (parameters.Length > 1 && parameters[1].ParameterType != typeof(TaskConfig))
                        return false;
                    if (method.ReturnType != typeof(void))
                        return false;
                    return true;
            }

            return true;
        }

        protected abstract Assembly ResolveAssembly(object sender, ResolveEventArgs args);
    }
}
