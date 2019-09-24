#pragma warning disable S3885 // "Assembly.Load" should be used

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Tasks.Core;

namespace Cake.Tasks.Module
{
    public abstract class PluginLoader
    {
        protected PluginLoader(string pluginsDir, ICakeLog log)
        {
            PluginsDir = pluginsDir;
            Log = log;
        }

        internal List<RegisteredTask> RegisteredTasks { get; } = new List<RegisteredTask>();

        public abstract void LoadPlugins();

        protected string PluginsDir { get; }

        protected ICakeLog Log { get; }

        protected void FindPlugins(string dllFile)
        {
            Assembly assembly = Assembly.LoadFile(dllFile);

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
                    RegisteredTasks.Add(registeredTask);

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
                            registeredTask.Name = $"{namePrefix}{attr.CoreTask}_{method.Name}";
                            break;
                        case ConfigAttribute attr:
                            string uniqueId = Guid.NewGuid().ToString("N");
                            string envSuffix = attr.Environment is null ? string.Empty : $"_{attr.Environment}";
                            registeredTask.Name = $"Config_{uniqueId}{envSuffix}";
                            registeredTask.Order = attr.Order;
                            break;
                    }
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

    public sealed class ProductionPluginLoader : PluginLoader
    {
        public ProductionPluginLoader(string pluginsDir, ICakeLog log)
            : base(pluginsDir, log)
        {
        }

        public override void LoadPlugins()
        {
            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;

            string[] cakeTasksDirs = Directory.GetDirectories(PluginsDir, "Cake.Tasks.*", SearchOption.TopDirectoryOnly);
            foreach (string cakeTasksDir in cakeTasksDirs)
            {
                string dllDir = Path.Combine(cakeTasksDir, "lib", "netstandard2.0");
                if (!Directory.Exists(dllDir))
                    continue;

                string[] dllFiles = Directory.GetFiles(dllDir, "*.dll", SearchOption.TopDirectoryOnly);

                foreach (string dllFile in dllFiles)
                    FindPlugins(dllFile);
            }
        }

        protected override Assembly ResolveAssembly(object sender, ResolveEventArgs args)
        {
            var assemblyName = new AssemblyName(args.Name);
            string assemblyPath = Directory
                .GetFiles(PluginsDir, $"{assemblyName.Name}.dll", SearchOption.AllDirectories)
                .FirstOrDefault();
            Log.Verbose($"[Assembly Lookup] {assemblyName.Name}.dll");
            return Assembly.LoadFile(assemblyPath);
        }
    }

    public sealed class DebugPluginLoader : PluginLoader
    {
        public DebugPluginLoader(string pluginsDir, ICakeLog log)
            : base(pluginsDir, log)
        {
        }

        public override void LoadPlugins()
        {
            string[] dllFiles = Directory.GetFiles(PluginsDir, "Cake.Tasks.*.dll", SearchOption.TopDirectoryOnly);

            foreach (string dllFile in dllFiles)
                FindPlugins(dllFile);
        }

        protected override Assembly ResolveAssembly(object sender, ResolveEventArgs args)
        {
            var assemblyName = new AssemblyName(args.Name);
            string assemblyPath = Directory
                .GetFiles(PluginsDir, $"{assemblyName.Name}.dll", SearchOption.TopDirectoryOnly)
                .FirstOrDefault();
            Log.Verbose($"[Assembly Lookup] {assemblyName.Name}.dll");
            return Assembly.LoadFile(assemblyPath);
        }
    }
}
