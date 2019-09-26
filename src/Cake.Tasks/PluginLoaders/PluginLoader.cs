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
                    TaskAttribute taskAttribute = method.GetCustomAttribute<TaskAttribute>(inherit: true);
                    if (taskAttribute is null)
                        continue;
                    if (!IsValidPluginMethod(method, taskAttribute))
                        continue;

                    Log.Verbose($"[Plugin Method] {taskPluginType.FullName}.{method.Name}");

                    var registeredTask = new RegisteredTask
                    {
                        AttributeType = taskAttribute.GetType(),
                        Method = method,
                        Environment = taskAttribute.Environment,
                    };

                    switch (taskAttribute)
                    {
                        case CoreTaskAttribute attr:
                            registeredTask.CoreTask = attr.CoreTask;
                            registeredTask.Name = attr.CoreTask.ToString();
                            break;
                        case TaskEventAttribute attr:
                            registeredTask.CoreTask = attr.CoreTask;
                            registeredTask.EventType = attr.EventType;
                            string namePrefix = attr.EventType == TaskEventType.BeforeTask ? "Before" : "After";
                            registeredTask.Name = $"{namePrefix}{attr.CoreTask}-{method.Name}";
                            break;
                        case ConfigAttribute attr:
                            string uniqueId = Guid.NewGuid().ToString("N");
                            string envSuffix = attr.Environment is null ? string.Empty : $"-{attr.Environment}";
                            registeredTask.Name = $"Config-{uniqueId}{envSuffix}";
                            registeredTask.Order = attr.Order;
                            break;
                    }

                    yield return registeredTask;
                }
            }
        }

        protected bool IsValidPluginMethod(MethodInfo method, TaskAttribute attribute)
        {
            ParameterInfo[] parameters = method.GetParameters();
            switch (attribute)
            {
                case CoreTaskAttribute _:
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
