#pragma warning disable S3885 // "Assembly.Load" should be used

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using Cake.Common.IO;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Tasks.Config;
using Cake.Tasks.Core;
using Cake.Tasks.Module.PluginLoaders;

namespace Cake.Tasks.Module
{
    public sealed partial class TasksEngine
    {
        private List<RegisteredTask> _registeredTasks;

        private void InitializeCakeTasksSystem()
        {
            InitializeConfiguration();
            DiscoverPluginTasks();
            RegisterPluginTasks();
            RegisterBuiltInTasks();
            RegisterCiTasks();
        }

        private void InitializeConfiguration()
        {
            RegisterSetupAction(ctx =>
            {
                Log.Information("Initializing Cake.Tasks configuration.");

                var config = TaskConfig.Current;

                var env = config.Load<EnvConfig>();
                env.Configuration = ctx.Arguments.GetArgument("Configuration") ?? "Release";
                env.IsCi = false;
                env.WorkingDirectory = ctx.Environment.WorkingDirectory.FullPath;

                var ci = config.Load<CiConfig>();
                string outputDirectory = Path.Combine(env.WorkingDirectory, "__output");
                ci.ArtifactsDirectory = Path.Combine(outputDirectory, "artifacts");
                ci.BuildOutputDirectory = Path.Combine(outputDirectory, "build");
                ci.TestOutputDirectory = Path.Combine(outputDirectory, "testresults");
                ci.BuildNumber = 1;
                ci.Version = "0.1.0";

                return config;
            });
        }

        private void DiscoverPluginTasks()
        {
            // Figure out the plugins root directory
            string pluginsDirKey = Context.Configuration.GetValue("CakeTasks_PluginsDirKey") ?? "Paths_Addins";
            string pluginsDir = Path.GetFullPath(Context.Configuration.GetValue(pluginsDirKey));

            // Figure out the plugin loader type and create an instance
            string pluginLoaderClassName = Context.Configuration.GetValue("CakeTasks_PluginLoader") ?? "ProductionPluginLoader";
            Type pluginLoaderType = Assembly.GetExecutingAssembly().GetExportedTypes()
                .SingleOrDefault(type => type.Name.Equals(pluginLoaderClassName, StringComparison.OrdinalIgnoreCase));
            var pluginLoader = (PluginLoader)Activator.CreateInstance(pluginLoaderType, pluginsDir, Log);

            // Discover plugins from the plugins loader
            _registeredTasks = pluginLoader.LoadPlugins().ToList();
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

                // For config tasks, add a description
                if (registeredTask.AttributeType == typeof(ConfigAttribute))
                    builder.Description($"Config for {registeredTask.Method.Name} from {registeredTask.Method.DeclaringType.FullName} ({registeredTask.Method.DeclaringType.Assembly.GetName().Name})");
            }
        }

        private void RegisterBuiltInTasks()
        {
            void RegisterDefaultTask()
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
            }

            void RegisterListEnvsTask()
            {
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
            }

            void RegisterListConfigsTask()
            {
                CakeTaskBuilder listConfigsTask = RegisterTask("List-Configs")
                    .Description("Lists all available configurations.");
                IEnumerable<RegisteredTask> configTasks = _registeredTasks.Where(rt => rt.AttributeType == typeof(ConfigAttribute));
                foreach (RegisteredTask configTask in configTasks)
                    listConfigsTask = listConfigsTask.IsDependentOn(configTask.Name);
                listConfigsTask = listConfigsTask.IsDependentOn("Config-Finalize");
                listConfigsTask.Does<TaskConfig>((context, config) =>
                {
                    context.Log.Information("Available Configurations");
                    context.Log.Information("------------------------");
                    foreach (KeyValuePair<string, object> kvp in config.Data.OrderBy(kvp => kvp.Key))
                        context.Log.Information($"{kvp.Key} = {kvp.Value?.ToString() ?? "[NULL]"}");
                });
            }

            void RegisterDeferredSetupTask()
            {
                RegisterTask("Config-Finalize")
                    .Does(ctx =>
                    {
                        TaskConfig config = TaskConfig.Current;

                        // Run configurations specified in the build.cake file
                        config.PerformDeferredSetup();

                        // Override configurations from environment variables
                        IDictionary<string, string> envVars = ctx.Environment.GetEnvironmentVariables();
                        foreach (var envVar in envVars)
                        {
                            if (config.Data.ContainsKey(envVar.Key))
                                config.Data[envVar.Key] = envVar.Value;
                        }

                        // Override configurations from command line arguments
                        List<string> keys = config.Data.Keys.ToList();
                        for (int i = 0; i < keys.Count; i++)
                        {
                            string key = keys[i];
                            if (ctx.Arguments.HasArgument(key))
                                config.Data[key] = ctx.Arguments.GetArgument(key);
                        }

                        // Display the final configuration values
                        ctx.Log.Information("Final Configurations");
                        ctx.Log.Information("--------------------");
                        foreach (var data in config.Data.OrderBy(kvp => kvp.Key))
                            ctx.Log.Information($"{data.Key} = {data.Value?.ToString() ?? "[NULL]"}");

                        // Clean out output directories or create them
                        var ci = config.Load<CiConfig>();
                        if (ctx.DirectoryExists(ci.ArtifactsDirectory))
                            ctx.CleanDirectory(ci.ArtifactsDirectory);
                        else
                            ctx.CreateDirectory(ci.ArtifactsDirectory);
                        if (ctx.DirectoryExists(ci.BuildOutputDirectory))
                            ctx.CleanDirectory(ci.BuildOutputDirectory);
                        else
                            ctx.CreateDirectory(ci.BuildOutputDirectory);
                        if (ctx.DirectoryExists(ci.TestOutputDirectory))
                            ctx.CleanDirectory(ci.TestOutputDirectory);
                        else
                            ctx.CreateDirectory(ci.TestOutputDirectory);
                    });
            }

            RegisterDefaultTask();
            RegisterListEnvsTask();
            RegisterListConfigsTask();
            RegisterDeferredSetupTask();
        }

        private void RegisterCiTasks()
        {
            string env = Context.Arguments.GetArgument("env");
            IList<RegisteredTask> envTasks = string.IsNullOrEmpty(env)
                ? _registeredTasks.Where(rt => rt.Environment is null).ToList()
                : _registeredTasks.Where(rt => rt.Environment is null || rt.Environment.Equals(env, StringComparison.OrdinalIgnoreCase)).ToList();

            CakeTaskBuilder ciTask = RegisterTask("CI").Description("Performs CI (Build and test)");

            // Config dependencies
            IEnumerable<RegisteredTask> configTasks = envTasks
                .Where(task => task.AttributeType == typeof(ConfigAttribute))
                .OrderBy(task => task.Order);
            foreach (RegisteredTask configTask in configTasks)
                ciTask = ciTask.IsDependentOn(configTask.Name);

            ciTask = ciTask.IsDependentOn("Config-Finalize");

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

        private void BuildTaskChain(CakeTaskBuilder builder, RegisteredTask coreTask, IList<RegisteredTask> envTasks)
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
}
