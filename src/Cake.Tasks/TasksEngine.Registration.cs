#pragma warning disable S3885 // "Assembly.Load" should be used

using System;
using System.Collections;
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
        private readonly List<RegisteredTask> _registeredTasks = new List<RegisteredTask>();
        private string _addinsPath;

        private void RegisterPlugins()
        {
            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;

            _addinsPath = Path.GetFullPath(Context.Configuration.GetValue("Paths_AddIns"));
            Log.Verbose($"Searching addins path: {_addinsPath}");

            RegisterBuiltInTasks();
            InitializeConfiguration();
            FindPluginPackages();
            RegisterPluginTasks();
            CreateCiTasks();
        }

        private void RegisterBuiltInTasks()
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

            RegisterTask("List-Envs")
                .Description("Lists all available environments.")
                .Does(context =>
                {
                    IList<string> environments = _registeredTasks
                        .Select(task => task.Environment)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .OrderBy(env => env)
                        .ToList();

                    if (environments.Count > 0)
                    {
                        context.Log.Information("Available Environments");
                        context.Log.Information("======================");
                        foreach (string env in environments)
                            context.Log.Information(env);
                    }
                    else
                        context.Log.Information("No environments found!");
                });

            RegisterTask("List-Configs")
                .Description("Lists all available configurations.")
                .Does<TaskConfig>((context, config) =>
                {
                    context.Log.Information("Available Configurations");
                    context.Log.Information("========================");
                    foreach (KeyValuePair<string, TaskConfigValue> kvp in config.Data)
                    {
                        context.Log.Information($"{kvp.Key} = ${kvp.Value?.ToString() ?? "[NULL]"}");
                    }
                });
        }

        private void InitializeConfiguration()
        {
            RegisterSetupAction(ctx =>
            {
                var config = new TaskConfig();

                config.Register("ENV_WorkingDirectory", ctx.Environment.WorkingDirectory.FullPath);

                IDictionary envVars = Environment.GetEnvironmentVariables();
                foreach (DictionaryEntry envVar in envVars)
                {
                    string key = envVar.Key?.ToString();
                    string value = envVar.Value.ToString() ?? string.Empty;

                    //config.Register($"ENV_{envVar}", envVars[envVar].ToString());
                    if (!string.IsNullOrWhiteSpace(key))
                        config.Register($"ENV_{key}", value);
                }

                return config;
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

        private bool IsValidPluginMethod(MethodInfo method, TaskAttribute attribute)
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

        private Assembly ResolveAssembly(object sender, ResolveEventArgs args)
        {
            var assemblyName = new AssemblyName(args.Name);
            string assemblyPath = Directory
                .GetFiles(_addinsPath, $"{assemblyName.Name}.dll", SearchOption.AllDirectories)
                .FirstOrDefault();
            Log.Verbose($"[Assembly Lookup] {assemblyName.Name}.dll");
            return Assembly.LoadFile(assemblyPath);
        }

        private void RegisterPluginTasks()
        {
            foreach (RegisteredTask registeredTask in _registeredTasks)
            {
                if (registeredTask.Method.GetParameters().Length == 2)
                {
                    var action = (Action<ICakeContext, TaskConfig>)registeredTask.Method.CreateDelegate(typeof(Action<ICakeContext, TaskConfig>));
                    RegisterTask(registeredTask.Name).Does(action);
                }
                else
                {
                    var action = (Action<ICakeContext>)registeredTask.Method.CreateDelegate(typeof(Action<ICakeContext>));
                    RegisterTask(registeredTask.Name).Does(action);
                }
            }
        }

        private void CreateCiTasks()
        {
            string env = Context.Arguments.GetArgument("env");
            IEnumerable<RegisteredTask> envTasks;
            if (string.IsNullOrEmpty(env))
                envTasks = _registeredTasks.Where(rt => rt.Environment is null);
            else
                envTasks = _registeredTasks.Where(rt => rt.Environment is null || rt.Environment.Equals(env, StringComparison.OrdinalIgnoreCase));

            CakeTaskBuilder ciTask = RegisterTask("CI").Description("Performs CI (Build and Test)");

            // Config dependencies
            IEnumerable<RegisteredTask> configTasks = envTasks
                .Where(task => task.AttributeType == typeof(ConfigAttribute))
                .OrderBy(task => task.Order);
            foreach (RegisteredTask configTask in configTasks)
                ciTask = ciTask.IsDependentOn(configTask.Name);

            // Build task
            RegisteredTask buildTask = envTasks
                .SingleOrDefault(task => task.AttributeType == typeof(CoreTaskAttribute) && task.CoreTask == CoreTask.Build);
            BuildTaskChain(ciTask, buildTask, envTasks);

            // Test tasl
            RegisteredTask testTask = envTasks
                .SingleOrDefault(task => task.AttributeType == typeof(CoreTaskAttribute) && task.CoreTask == CoreTask.Test);
            BuildTaskChain(ciTask, testTask, envTasks);

            ciTask.Does(() => { });

            // Deploy task
            RegisteredTask deployTask = envTasks
                .SingleOrDefault(task => task.AttributeType == typeof(CoreTaskAttribute) && task.CoreTask == CoreTask.Deploy);
            if (deployTask != null)
            {
                CakeTaskBuilder cicdTask = RegisterTask("CICD")
                    .Description("Performs CD/CD (Build, test and deploy)")
                    .IsDependentOn("CI");

                BuildTaskChain(cicdTask, deployTask, envTasks);

                cicdTask.Does(() => { });
            }
        }

        private void BuildTaskChain(CakeTaskBuilder builder, RegisteredTask coreTask, IEnumerable<RegisteredTask> envTasks)
        {
            if (coreTask is null)
                return;

            // Add before tasks
            IEnumerable<RegisteredTask> beforeTasks = envTasks
                .Where(task => task.AttributeType == typeof(TaskEventAttribute) && task.CoreTask == coreTask.CoreTask && task.EventType == TaskEventType.BeforeTask);
            foreach (RegisteredTask beforeTask in beforeTasks)
                builder = builder.IsDependentOn(beforeTask.Name);

            // Add core task
            builder = builder.IsDependentOn(coreTask.Name);

            // Add after tasks
            IEnumerable<RegisteredTask> afterTasks = envTasks
                .Where(task => task.AttributeType == typeof(TaskEventAttribute) && task.CoreTask == coreTask.CoreTask && task.EventType == TaskEventType.AfterTask);
            foreach (RegisteredTask afterTask in afterTasks)
                builder = builder.IsDependentOn(afterTask.Name);
        }
    }

    internal sealed class RegisteredTask
    {
        internal Type AttributeType { get; set; }

        internal MethodInfo Method { get; set; }

        internal string Name { get; set; }

        internal string Environment { get; set; }

        // Optional properties - specific to task type

        internal CoreTask? CoreTask { get; set; }

        internal TaskEventType? EventType { get; set; }

        internal int Order { get; set; }
    }
}
