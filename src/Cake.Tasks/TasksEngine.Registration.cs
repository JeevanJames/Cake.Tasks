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
    public sealed partial class TasksEngine
    {
        private string _addinsPath;
        private List<RegisteredTask> _registeredTasks = new List<RegisteredTask>();

        private void RegisterPlugins()
        {
            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;

            _addinsPath = Path.GetFullPath(Context.Configuration.GetValue("Paths_AddIns"));
            Log.Verbose($"Searching addins path: {_addinsPath}");

            RegisterDefaultTask();
            RegisterConfiguration();
            FindPluginPackages();
            RegisterTasks();
            RegisterCiTask();
        }

        private void RegisterDefaultTask()
        {
            RegisterTask("Default")
                .Does(context =>
                {
                    context.Log.Information("Task List");
                    context.Log.Information("=========");
                    foreach (ICakeTaskInfo task in Tasks)
                    {
                        context.Log.Information(task.Name);
                        if (!string.IsNullOrWhiteSpace(task.Description))
                            context.Log.Information($"    {task.Description}");
                    }
                });
        }

        private void RegisterConfiguration()
        {
            RegisterSetupAction(ctx =>
            {
                return new TaskConfig();
            });
        }

        private void FindPluginPackages()
        {
            string[] cakeTasksDirs = Directory.GetDirectories(_addinsPath, "Cake.Tasks.*", SearchOption.TopDirectoryOnly);
            foreach (string cakeTasksDir in cakeTasksDirs)
            {
                Log.Verbose($"[Plugin Dir] {Path.GetFileName(cakeTasksDir)}");

                string dllDir = Path.Combine(cakeTasksDir, "lib", "netstandard2.0");
                string[] dllFiles = Directory.GetFiles(dllDir, "*.dll", SearchOption.TopDirectoryOnly);

                foreach (string dllFile in dllFiles)
                    FindPlugins(dllFile);
            }
        }

        private void FindPlugins(string dllFile)
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
                    _registeredTasks.Add(registeredTask);

                    switch (taskAttribute)
                    {
                        case CoreTaskAttribute attr:
                            registeredTask.CoreTask = attr.CoreTask;
                            registeredTask.Name = attr.CoreTask.ToString();
                            break;
                        case PreTaskAttribute attr:
                            registeredTask.CoreTask = attr.CoreTask;
                            registeredTask.Name = $"Before{attr.CoreTask.ToString()}_{attr.Name}";
                            break;
                        case PostTaskAttribute attr:
                            registeredTask.CoreTask = attr.CoreTask;
                            registeredTask.Name = $"After{attr.CoreTask.ToString()}_{attr.Name}";
                            break;
                        case ConfigAttribute attr:
                            registeredTask.Name = $"Config_{attr.Environment}";
                            break;
                    }
                }
            }
        }

        private bool IsValidPluginMethod(MethodInfo method, TaskAttribute attribute)
        {
            ParameterInfo[] parameters = method.GetParameters();
            switch (attribute)
            {
                case CoreTaskAttribute _:
                case PreTaskAttribute _:
                case PostTaskAttribute _:
                    if (parameters.Length != 2)
                        return false;
                    if (!typeof(ICakeContext).IsAssignableFrom(parameters[0].ParameterType))
                        return false;
                    if (parameters[1].ParameterType != typeof(TaskConfig))
                        return false;
                    if (method.ReturnType != typeof(void))
                        return false;
                    return true;
            }

            return true;
        }

        private Assembly ResolveAssembly(object sender, ResolveEventArgs args)
        {
            var assemblyName = new AssemblyName(args.Name);
            string assemblyPath = Directory
                .GetFiles(_addinsPath, $"{assemblyName.Name}.dll", SearchOption.AllDirectories)
                .FirstOrDefault();
            Log.Verbose($"[Assembly Lookup] {assemblyName.Name}.dll");
            return Assembly.LoadFile(assemblyPath);
        }

        private void RegisterTasks()
        {
            foreach (RegisteredTask registeredTask in _registeredTasks)
            {
                var action = (Action<ICakeContext, TaskConfig>)registeredTask.Method.CreateDelegate(typeof(Action<ICakeContext, TaskConfig>));
                RegisterTask(registeredTask.Name).Does(action);
            }
        }

        private static readonly List<CoreTask> CoreTasks = new List<CoreTask> { CoreTask.Build, CoreTask.Test, CoreTask.Deploy };

        private void RegisterCiTask()
        {
            string env = Context.Arguments.GetArgument("env");

            IEnumerable<RegisteredTask> filteredTasks;
            if (string.IsNullOrEmpty(env))
                filteredTasks = _registeredTasks.Where(rt => rt.Environment is null);
            else
                filteredTasks = _registeredTasks.Where(rt => rt.Environment is null || rt.Environment.Equals(env, StringComparison.OrdinalIgnoreCase));

            CakeTaskBuilder builder = RegisterTask("CI");

            IEnumerable<RegisteredTask> configTasks = filteredTasks.Where(task => task.AttributeType == typeof(ConfigAttribute));
            foreach (RegisteredTask configTask in configTasks)
                builder = builder.IsDependentOn(configTask.Name);

            foreach (CoreTask coreTask in CoreTasks)
            {
                RegisteredTask registeredCoreTask = filteredTasks.SingleOrDefault(task => task.AttributeType == typeof(CoreTaskAttribute) && task.CoreTask == coreTask);
                if (registeredCoreTask is null)
                    continue;

                builder = builder.IsDependentOn(registeredCoreTask.Name);

                IEnumerable<RegisteredTask> preTasks = filteredTasks.Where(task => task.AttributeType == typeof(PreTaskAttribute) && task.CoreTask == coreTask);
                foreach (RegisteredTask preTask in preTasks)
                    builder = builder.IsDependentOn(preTask.Name);

                IEnumerable<RegisteredTask> postTasks = filteredTasks.Where(task => task.AttributeType == typeof(PostTaskAttribute) && task.CoreTask == coreTask);
                foreach (RegisteredTask postTask in postTasks)
                    builder = builder.IsDependentOn(postTask.Name);
            }

            builder.Does(() => { });
        }
    }

    internal sealed class RegisteredTask
    {
        internal Type AttributeType { get; set; }

        internal MethodInfo Method { get; set; }

        internal CoreTask? CoreTask { get; set; }

        internal string Name { get; set; }

        internal string Environment { get; set; }
    }
}
