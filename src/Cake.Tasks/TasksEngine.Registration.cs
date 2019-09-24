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
        private List<RegisteredTask> _registeredTasks;

        private void RegisterPlugins()
        {
            InitializeConfiguration();

            string pluginsDirKey = Context.Configuration.GetValue("CakeTasks_PluginsDirKey") ?? "Paths_Addins";
            string pluginsDir = Path.GetFullPath(Context.Configuration.GetValue(pluginsDirKey));
            string pluginLoaderClassName = Context.Configuration.GetValue("CakeTasks_PluginLoader") ?? "ProductionPluginLoader";
            Type pluginLoaderType = Assembly.GetExecutingAssembly().GetExportedTypes()
                .SingleOrDefault(type => type.Name.Equals(pluginLoaderClassName, StringComparison.OrdinalIgnoreCase));
            var pluginLoader = (PluginLoader)Activator.CreateInstance(pluginLoaderType, pluginsDir, Log);
            pluginLoader.LoadPlugins();
            _registeredTasks = pluginLoader.RegisteredTasks;

            RegisterPluginTasks();
            RegisterBuiltInTasks();
            CreateCiTasks();
        }

        private void RegisterBuiltInTasks()
        {
            RegisterTask("Default")
                .Does(context =>
                {
                    context.Log.Information("Task List");
                    context.Log.Information("---------");
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
                        context.Log.Information("----------------------");
                        foreach (string env in environments)
                            context.Log.Information(env);
                    }
                    else
                        context.Log.Information("No environments found!");
                });

            CakeTaskBuilder listConfigsTask = RegisterTask("List-Configs")
                .Description("Lists all available configurations.");
            IEnumerable<RegisteredTask> configTasks = _registeredTasks.Where(rt => rt.AttributeType == typeof(ConfigAttribute));
            foreach (RegisteredTask configTask in configTasks)
                listConfigsTask = listConfigsTask.IsDependentOn(configTask.Name);
            listConfigsTask = listConfigsTask.IsDependentOn("Config_DeferredSetup");
            listConfigsTask.Does<TaskConfig>((context, config) =>
            {
                context.Log.Information("Available Configurations");
                context.Log.Information("------------------------");
                foreach (KeyValuePair<string, object> kvp in config.Data.OrderBy(kvp => kvp.Key))
                    context.Log.Information($"{kvp.Key} = {kvp.Value?.ToString() ?? "[NULL]"}");
            });

            RegisterTask("Config_DeferredSetup")
                .Does(() => TaskConfig.Current.PerformDeferredSetup());
        }

        private void InitializeConfiguration()
        {
            RegisterSetupAction(ctx =>
            {
                var config = TaskConfig.Current;

                var env = config.Load<EnvConfig>();
                env.WorkingDirectory = ctx.Environment.WorkingDirectory.FullPath;

                return config;
            });
        }

        private void RegisterPluginTasks()
        {
            foreach (RegisteredTask registeredTask in _registeredTasks)
            {
                CakeTaskBuilder builder;

                if (registeredTask.Method.GetParameters().Length == 2)
                {
                    var action = (Action<ICakeContext, TaskConfig>)registeredTask.Method.CreateDelegate(typeof(Action<ICakeContext, TaskConfig>));
                    builder = RegisterTask(registeredTask.Name).Does(action);
                }
                else
                {
                    var action = (Action<ICakeContext>)registeredTask.Method.CreateDelegate(typeof(Action<ICakeContext>));
                    builder = RegisterTask(registeredTask.Name).Does(action);
                }

                if (registeredTask.AttributeType == typeof(ConfigAttribute))
                    builder.Description($"Config for {registeredTask.Method.Name} from {registeredTask.Method.DeclaringType.FullName} ({registeredTask.Method.DeclaringType.Assembly.GetName().Name})");
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

            ciTask = ciTask.IsDependentOn("Config_DeferredSetup");

            // Build task
            RegisteredTask buildTask = envTasks
                .SingleOrDefault(task => task.AttributeType == typeof(CoreTaskAttribute) && task.CoreTask == CoreTask.Build);
            BuildTaskChain(ciTask, buildTask, envTasks);

            // Test task
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
