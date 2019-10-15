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
            RegisterPipelineTasks();
        }

        private void InitializeConfiguration()
        {
            RegisterSetupAction(ctx =>
            {
                Log.Verbose("Initializing Cake.Tasks configuration.");

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
                ci.Version = (Func<string>)(() => $"0.{ci.BuildNumber}.0");

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
                CakeTaskBuilder builder = RegisterTask(registeredTask.Name);

                if (registeredTask.Method.GetParameters().Length == 2)
                {
                    var action = (Action<ICakeContext, TaskConfig>)registeredTask.Method.CreateDelegate(typeof(Action<ICakeContext, TaskConfig>));
                    builder.Does(action);
                }
                else
                {
                    var action = (Action<ICakeContext>)registeredTask.Method.CreateDelegate(typeof(Action<ICakeContext>));
                    builder.Does(action);
                }

                // Add a description
                string taskType;
                if (registeredTask.AttributeType == typeof(ConfigAttribute))
                    taskType = "Config task";
                else if (registeredTask.AttributeType == typeof(TaskEventAttribute))
                    taskType = $"{(registeredTask.EventType == TaskEventType.BeforeTask ? "Before" : "After")} {registeredTask.CoreTask} task";
                else if (registeredTask.AttributeType == typeof(PipelineTaskAttribute))
                    taskType = $"{registeredTask.CoreTask} task";
                else
                    throw new NotSupportedException($"Unrecognized task type: {registeredTask.AttributeType.FullName}");

                builder.Description($"{taskType} - {registeredTask.Method.DeclaringType.FullName}.{registeredTask.Method.Name} ({registeredTask.Method.DeclaringType.Assembly.GetName().Name})");
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
                            if (task.Name.StartsWith("_"))
                                continue;
                            if (task.Name.Equals("Default", StringComparison.OrdinalIgnoreCase))
                                continue;

                            context.Log.Information(task.Name);
                            if (!string.IsNullOrWhiteSpace(task.Description))
                                context.Log.Information($"    {task.Description}");
                        }
                    });
            }

            void RegisterListEnvsTask()
            {
                RegisterTask(TaskNames.ListEnvs)
                    .Description("Lists all available environments.")
                    .Does(context =>
                    {
                        IList<string> environments = _registeredTasks
                            .Select(task => task.CiSystem)
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
                CakeTaskBuilder listConfigsTask = RegisterTask(TaskNames.ListConfigs)
                    .Description("Lists all available configurations.");
                IEnumerable<RegisteredTask> configTasks = GetTasksForCiEnvironment()
                    .Where(rt => rt.AttributeType == typeof(ConfigAttribute));
                foreach (RegisteredTask configTask in configTasks)
                    listConfigsTask = listConfigsTask.IsDependentOn(configTask.Name);
                listConfigsTask = listConfigsTask.IsDependentOn(TaskNames.Config);
                listConfigsTask.Does<TaskConfig>((context, config) =>
                {
                    context.Log.Information("Available Configurations");
                    context.Log.Information("------------------------");
                    foreach (KeyValuePair<string, object> kvp in config.Data.OrderBy(kvp => kvp.Key))
                        context.Log.Information($"{kvp.Key} = {kvp.Value?.ToString() ?? "[NULL]"}");
                });
            }

            void RegisterListTasksTask()
            {
                RegisterTask(TaskNames.ListTasks)
                    .Description("Lists all tasks including private tasks.")
                    .Does(context =>
                    {
                        context.Log.Information("Task List");
                        context.Log.Information("---------");
                        foreach (ICakeTaskInfo task in Tasks)
                        {
                            if (task.Name.Equals("Default", StringComparison.OrdinalIgnoreCase))
                                continue;

                            context.Log.Information(task.Name);
                            if (!string.IsNullOrWhiteSpace(task.Description))
                                context.Log.Information($"    {task.Description}");
                        }
                    });
            }

            RegisterDefaultTask();
            RegisterListEnvsTask();
            RegisterListConfigsTask();
            RegisterListTasksTask();
        }

        private void RegisterPipelineTasks()
        {
            IReadOnlyList<RegisteredTask> envTasks = GetTasksForCiEnvironment();

            RegisterConfigTask(envTasks);
            RegisterBuildTask(envTasks);
            RegisterTestTask(envTasks);
            RegisterCiTask();
            RegisterCicdTask(envTasks);
        }

        private void RegisterConfigTask(IReadOnlyList<RegisteredTask> envTasks)
        {
            CakeTaskBuilder task = RegisterTask(TaskNames.Config);

            IEnumerable<RegisteredTask> configTasks = envTasks
                .Where(t => t.AttributeType == typeof(ConfigAttribute))
                .OrderBy(t => t.Order);

            foreach (RegisteredTask configTask in configTasks)
                task.IsDependentOn(configTask.Name);

            //task.IsDependentOn(TaskNames.ConfigFinalize);

            task.Does(ctx =>
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

        private void RegisterBuildTask(IReadOnlyList<RegisteredTask> envTasks)
        {
            CakeTaskBuilder task = RegisterTask(TaskNames.Build)
                .Description("Builds the solution.")
                .IsDependentOn(TaskNames.Config);

            IEnumerable<RegisteredTask> preBuildTasks = envTasks
                .Where(t => t.AttributeType == typeof(TaskEventAttribute) && t.CoreTask == PipelineTask.Build && t.EventType == TaskEventType.BeforeTask);
            foreach (RegisteredTask preBuildTask in preBuildTasks)
                task.IsDependentOn(preBuildTask.Name);

            IEnumerable<RegisteredTask> buildTasks = envTasks
                .Where(t => t.AttributeType == typeof(PipelineTaskAttribute) && t.CoreTask == PipelineTask.Build);
            foreach (RegisteredTask buildTask in buildTasks)
                task.IsDependentOn(buildTask.Name);

            IEnumerable<RegisteredTask> postBuildTasks = envTasks
                .Where(t => t.AttributeType == typeof(TaskEventAttribute) && t.CoreTask == PipelineTask.Build && t.EventType == TaskEventType.AfterTask);
            foreach (RegisteredTask postBuildTask in postBuildTasks)
                task.IsDependentOn(postBuildTask.Name);
        }

        private void RegisterTestTask(IReadOnlyList<RegisteredTask> envTasks)
        {
            CakeTaskBuilder task = RegisterTask(TaskNames.Test)
                .Description("Runs tests in the project.")
                .IsDependentOn(TaskNames.Build);

            IEnumerable<RegisteredTask> preTestTasks = envTasks
                .Where(t => t.AttributeType == typeof(TaskEventAttribute) && t.CoreTask == PipelineTask.Test && t.EventType == TaskEventType.BeforeTask);
            foreach (RegisteredTask preTestTask in preTestTasks)
                task.IsDependentOn(preTestTask.Name);

            IEnumerable<RegisteredTask> testTasks = envTasks
                .Where(t => t.AttributeType == typeof(PipelineTaskAttribute) && t.CoreTask == PipelineTask.Test);
            foreach (RegisteredTask testTask in testTasks)
                task.IsDependentOn(testTask.Name);

            IEnumerable<RegisteredTask> postTestTasks = envTasks
                .Where(t => t.AttributeType == typeof(TaskEventAttribute) && t.CoreTask == PipelineTask.Test && t.EventType == TaskEventType.AfterTask);
            foreach (RegisteredTask postTestTask in postTestTasks)
                task.IsDependentOn(postTestTask.Name);
        }

        private void RegisterCiTask()
        {
            RegisterTask(TaskNames.Ci)
                .Description("Performs CI (Build and test)")
                .IsDependentOn(TaskNames.Test);
        }

        private void RegisterCicdTask(IReadOnlyList<RegisteredTask> envTasks)
        {
            CakeTaskBuilder task = RegisterTask("CICD")
                .Description("Performs CD/CD (Build, test and deploy)")
                .IsDependentOn("CI");

            IEnumerable<RegisteredTask> preDeployTasks = envTasks
                .Where(t => t.AttributeType == typeof(TaskEventAttribute) && t.CoreTask == PipelineTask.Deploy && t.EventType == TaskEventType.BeforeTask);
            foreach (RegisteredTask preDeployTask in preDeployTasks)
                task.IsDependentOn(preDeployTask.Name);

            IEnumerable<RegisteredTask> deployTasks = envTasks
                .Where(t => t.AttributeType == typeof(PipelineTaskAttribute) && t.CoreTask == PipelineTask.Deploy);
            foreach (RegisteredTask deployTask in deployTasks)
                task.IsDependentOn(deployTask.Name);

            IEnumerable<RegisteredTask> postDeployTasks = envTasks
                .Where(t => t.AttributeType == typeof(TaskEventAttribute) && t.CoreTask == PipelineTask.Deploy && t.EventType == TaskEventType.AfterTask);
            foreach (RegisteredTask postDeployTask in postDeployTasks)
                task.IsDependentOn(postDeployTask.Name);
        }

        private IReadOnlyList<RegisteredTask> GetTasksForCiEnvironment()
        {
            string ciEnv = Context.Arguments.GetArgument("ci");
            if (!string.IsNullOrEmpty(ciEnv))
                Log.Information($"CI Environment specified: {ciEnv}");

            List<RegisteredTask> envTasks = string.IsNullOrEmpty(ciEnv)
                ? _registeredTasks.Where(rt => rt.CiSystem is null).ToList()
                : _registeredTasks.Where(rt => rt.CiSystem is null || rt.CiSystem.Equals(ciEnv, StringComparison.OrdinalIgnoreCase)).ToList();
            return envTasks;
        }

        internal static class TaskNames
        {
            internal const string Config = "_Config";
            internal const string ListConfigs = "List-Configs";
            internal const string ListEnvs = "List-Envs";
            internal const string ListTasks = "List-Tasks";

            internal const string Build = "Build";
            internal const string Test = "Test";
            internal const string Ci = "CI";
            internal const string CiCd = "CICD";
        }
    }
}
