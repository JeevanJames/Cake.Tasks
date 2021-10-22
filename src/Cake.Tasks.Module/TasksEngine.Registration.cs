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
using Cake.Tasks.Core.Internal;
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
            // Setup action to initialize some really core stuff, including directories and version
            // details.
            RegisterSetupAction(ctx =>
            {
                Log.Verbose("Initializing Cake.Tasks configuration.");

                TaskConfig config = TaskConfig.Current;

                EnvConfig env = config.Load<EnvConfig>();
                env.Name = Path.GetFileName(ctx.Environment.WorkingDirectory.FullPath)
                    .Replace('.', '-')
                    .Replace(' ', '-')
                    .Replace('_', '-')
                    .Trim();
                env.Configuration = ctx.Arguments.GetArgument("Configuration") ?? "Release";
                env.IsCi = false;
                env.Branch = null;

                env.Directories.Working = ctx.Environment.WorkingDirectory.FullPath;
                string outputDirectory = Path.Combine(env.Directories.Working, ".ci");
                env.Directories.Artifacts = Path.Combine(outputDirectory, "artifacts");
                env.Directories.BinaryOutput = Path.Combine(outputDirectory, "binaries");
                env.Directories.PublishOutput = Path.Combine(outputDirectory, "publish");
                env.Directories.TestOutput = Path.Combine(outputDirectory, "testresults");

                env.Version.BuildNumber = "1";
                env.Version.Primary = (Func<string>)(() => $"0.{env.Version.BuildNumber.Resolve()}.0");
                env.Version.Full = (Func<string>)(() => env.Version.Primary.Resolve());
                env.Version.Build = (Func<string>)(() => $"{env.Version.Full.Resolve()}+{env.Version.BuildNumber.Resolve()}");

                // Display the subdirectories under the tools/Addins directory.
                // To verify the versions of the addins and tools installed.
                // Useful for troubleshooting.
                //TODO: Make this a configuration
                ctx.LogHighlight("--------------------");
                ctx.LogHighlight("Addin subdirectories");
                ctx.LogHighlight("--------------------");
                string addinsBaseDir = Path.Combine(env.Directories.Working, "tools", "Addins");
                if (Directory.Exists(addinsBaseDir))
                {
                    IEnumerable<string> addinsDirs = AddinFinder.Find(addinsBaseDir);
                    IEnumerable<string> otherDirs = Directory.EnumerateDirectories(addinsBaseDir, "*", SearchOption.TopDirectoryOnly)
                        .Except(addinsDirs, StringComparer.OrdinalIgnoreCase);
                    foreach (string addinsDir in addinsDirs)
                        ctx.LogHighlight(Path.GetFileName(addinsDir));
                    foreach (string otherDir in otherDirs)
                        ctx.Log.Information(Path.GetFileName(otherDir));
                }

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

        /// <summary>
        ///     Creates an actual Cake task from the internal <see cref="RegisteredTask"/> structure.
        /// </summary>
        private void RegisterPluginTasks()
        {
            foreach (RegisteredTask registeredTask in _registeredTasks)
            {
                CakeTaskBuilder builder = RegisterTask(registeredTask.Name)
                    .Description(registeredTask.Description);

                if (registeredTask.ContinueOnError)
                    builder.ContinueOnError();

                if (registeredTask.AttributeType == typeof(TaskAttribute) && registeredTask.RequiresConfig)
                    builder.IsDependentOn(TaskNames.Config);

                ParameterInfo[] parameters = registeredTask.Method.GetParameters();
                switch (parameters.Length)
                {
                    case 2:
                        var action2 = (Action<ICakeContext, TaskConfig>)registeredTask.Method.CreateDelegate(
                            typeof(Action<ICakeContext, TaskConfig>));
                        builder.Does(action2);
                        break;

                    case 1:
                        if (parameters[0].ParameterType == typeof(TaskConfig))
                        {
                            Action<ICakeContext, TaskConfig> action1 = (_, cfg) =>
                            {
                                registeredTask.Method.Invoke(obj: null, new object[] { cfg });
                            };
                            builder.Does(action1);
                        }
                        else
                        {
                            var action1 = (Action<ICakeContext>)registeredTask.Method.CreateDelegate(typeof(Action<ICakeContext>));
                            builder.Does(action1);
                        }

                        break;

                    default:
                        var action = (Action)registeredTask.Method.CreateDelegate(typeof(Action));
                        builder.Does(action);
                        break;
                }
            }
        }

        /// <summary>
        ///     Registers built-in utility tasks that can be used to display various details about the
        ///     Cake tasks, such as listing environments, tasks and configurations.
        /// </summary>
        private void RegisterBuiltInTasks()
        {
            // The Default task lists all available public tasks (tasks not prefixed with underscore).
            void RegisterDefaultTask()
            {
                RegisterTask("Default")
                    .Does(ctx =>
                    {
                        ctx.Log.Information("Task List");
                        ctx.Log.Information("---------");
                        foreach (ICakeTaskInfo task in Tasks)
                        {
                            if (task.Name.StartsWith("_"))
                                continue;
                            if (task.Name.Equals("Default", StringComparison.OrdinalIgnoreCase))
                                continue;

                            ctx.Log.Information(task.Name);
                            if (!string.IsNullOrWhiteSpace(task.Description))
                                ctx.Log.Information($"    {task.Description}");
                        }
                    });
            }

            // The ListEnvs task goes through all available tasks and identifies all the available
            // environments.
            void RegisterListEnvsTask()
            {
                RegisterTask(TaskNames.ListEnvs)
                    .Description("Lists all available environments.")
                    .Does(ctx =>
                    {
                        IList<string> environments = _registeredTasks
                            .Select(task => task.CiSystem)
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .OrderBy(env => env)
                            .ToList();

                        if (environments.Count > 0)
                        {
                            ctx.Log.Information("Available Environments");
                            ctx.Log.Information("----------------------");
                            foreach (string env in environments)
                                ctx.Log.Information(env);
                        }
                        else
                            ctx.Log.Information("No environments found!");
                    });
            }

            // The ListConfigs task lists the final values of all configuration values. It depends on
            // the _Config task, as it needs to execute all the config tasks first, before it can get
            // the final vakues.
            void RegisterListConfigsTask()
            {
                CakeTaskBuilder listConfigsTask = RegisterTask(TaskNames.ListConfigs)
                    .Description("Lists all available configurations.");
                IEnumerable<RegisteredTask> configTasks = GetTasksForCiEnvironment()
                    .Where(rt => rt.AttributeType == typeof(ConfigAttribute));
                foreach (RegisteredTask configTask in configTasks)
                    listConfigsTask = listConfigsTask.IsDependentOn(configTask.Name);
                listConfigsTask.IsDependentOn(TaskNames.Config);
            }

            // The ListTasks tasks lists all tasks including private tasks (which are prefixed with an
            // underscore).
            void RegisterListTasksTask()
            {
                RegisterTask(TaskNames.ListTasks)
                    .Description("Lists all tasks including private tasks.")
                    .Does(ctx =>
                    {
                        ctx.Log.Information("Task List");
                        ctx.Log.Information("---------");
                        foreach (ICakeTaskInfo task in Tasks)
                        {
                            if (task.Name.Equals("Default", StringComparison.OrdinalIgnoreCase))
                                continue;

                            ctx.Log.Information(task.Name);
                            if (!string.IsNullOrWhiteSpace(task.Description))
                                ctx.Log.Information($"    {task.Description}");
                        }
                    });
            }

            RegisterDefaultTask();
            RegisterListEnvsTask();
            RegisterListConfigsTask();
            RegisterListTasksTask();
        }

        /// <summary>
        ///     Registers the built-in pipeline tasks, which are:
        ///     <list type="bullet">
        ///         <item>
        ///             <term>Build</term>
        ///             <description>Builds the project.</description>
        ///         </item>
        ///         <item>
        ///             <term>Test</term>
        ///             <description>Executes the project's unit tests.</description>
        ///         </item>
        ///         <item>
        ///             <term>Deploy</term>
        ///             <description>Packages and deploys the project.</description>
        ///         </item>
        ///         <item>
        ///             <term>IntegrationTest</term>
        ///             <description>Executes the project's integration tests.</description>
        ///         </item>
        ///         <item>
        ///             <term>CI</term>
        ///             <description>Runs Build + Test</description>
        ///         </item>
        ///         <item>
        ///             <term>CICD</term>
        ///             <description>Runs CI + Deploy</description>
        ///         </item>
        ///     </list>
        /// </summary>
        private void RegisterPipelineTasks()
        {
            IReadOnlyList<RegisteredTask> envTasks = GetTasksForCiEnvironment();

            RegisterConfigTask(envTasks);
            RegisterBuildTask(envTasks);
            RegisterTestTask(envTasks);
            RegisterCiTask();
            RegisterCicdTask(envTasks);
            RegisterIntegrationTestTask(envTasks);
        }

        /// <summary>
        ///     Registers a task that performs final setup of configuration after all plugin-specific
        ///     config tasks have been run.
        ///     <para/>
        ///     This includes:
        ///     1. Runs all configuration lambdas from the <c>ConfigureTask</c> methods in the
        ///     build.cake file.
        ///     2. Override configurations with any matching values from the environment.
        ///     3. Override configurations with any matching values from the command line.
        /// </summary>
        /// <param name="envTasks">List of all plugin tasks for the current CI environment.</param>
        private void RegisterConfigTask(IReadOnlyList<RegisteredTask> envTasks)
        {
            CakeTaskBuilder task = RegisterTask(TaskNames.Config)
                .Description("Finalizes configurations and displays final configuration values.");

            // Create dependency on all plugin configuration tasks.
            IEnumerable<RegisteredTask> configTasks = envTasks
                .Where(t => t.AttributeType == typeof(ConfigAttribute))
                .OrderBy(t => t.Order);

            foreach (RegisteredTask configTask in configTasks)
                task.IsDependentOn(configTask.Name);

            task.Does(ctx =>
            {
                TaskConfig config = TaskConfig.Current;

                // Run configurations specified in the build.cake file
                config.PerformDeferredSetup();

                // Override configurations from environment variables
                IDictionary<string, string> envVars = ctx.Environment.GetEnvironmentVariables();
                foreach (KeyValuePair<string, string> envVar in envVars)
                {
                    if (config.Data.ContainsKey(envVar.Key))
                        config.Data[envVar.Key] = envVar.Value;
                }

                // Override configurations from command line arguments
                var keys = config.Data.Keys.ToList();
                foreach (string key in keys)
                {
                    if (ctx.Arguments.HasArgument(key))
                        config.Data[key] = ctx.Arguments.GetArgument(key);
                }

                // Display the final configuration values
                ctx.LogHighlight("Final Configurations");
                ctx.LogHighlight("--------------------");
                foreach (KeyValuePair<string, object> data in config.Data.OrderBy(kvp => kvp.Key))
                    ctx.LogHighlight($"{data.Key} = {data.Value?.Dump() ?? "[NULL]"}");

                EnvConfig env = config.Load<EnvConfig>();

                // Clean out output directories or create them
                //TODO: Can these directories be created on-demand? For some project types like Angular,
                //these folders are ignored and the dist folder is used.
                ctx.EnsureDirectoryExists(env.Directories.Artifacts);
                ctx.EnsureDirectoryExists(env.Directories.BinaryOutput);
                ctx.EnsureDirectoryExists(env.Directories.TestOutput);
            });
        }

        /// <summary>
        ///     Registers a build task that depends on a sequence of pre-build tasks, build tasks and
        ///     post-build tasks.
        /// </summary>
        /// <param name="envTasks">List of all plugin tasks for the current CI environment.</param>
        private void RegisterBuildTask(IReadOnlyList<RegisteredTask> envTasks)
        {
            CakeTaskBuilder task = RegisterTask(TaskNames.Build)
                .Description("Builds the solution.")
                .IsDependentOn(TaskNames.Config);

            // Add pre-build tasks.
            IEnumerable<RegisteredTask> preBuildTasks = envTasks
                .Where(t => t.AttributeType == typeof(BeforePipelineTaskAttribute) && t.CoreTask == PipelineTask.Build);
            foreach (RegisteredTask preBuildTask in preBuildTasks)
                task.IsDependentOn(preBuildTask.Name);

            // Add build tasks.
            IEnumerable<RegisteredTask> buildTasks = envTasks
                .Where(t => t.AttributeType == typeof(PipelineTaskAttribute) && t.CoreTask == PipelineTask.Build);
            foreach (RegisteredTask buildTask in buildTasks)
                task.IsDependentOn(buildTask.Name);

            // Add post-build tasks.
            IEnumerable<RegisteredTask> postBuildTasks = envTasks
                .Where(t => t.AttributeType == typeof(AfterPipelineTaskAttribute) && t.CoreTask == PipelineTask.Build);
            foreach (RegisteredTask postBuildTask in postBuildTasks)
                task.IsDependentOn(postBuildTask.Name);
        }

        /// <summary>
        ///     Registers a test task that depends on a sequence of pre-test tasks, test tasks and
        ///     post-test tasks.
        /// </summary>
        /// <param name="envTasks">List of all plugin tasks for the current CI environment.</param>
        private void RegisterTestTask(IReadOnlyList<RegisteredTask> envTasks)
        {
            CakeTaskBuilder task = RegisterTask(TaskNames.Test)
                .Description("Runs tests in the project.")
                .IsDependentOn(TaskNames.Build);

            IEnumerable<RegisteredTask> preTestTasks = envTasks
                .Where(t => t.AttributeType == typeof(BeforePipelineTaskAttribute) && t.CoreTask == PipelineTask.Test);
            foreach (RegisteredTask preTestTask in preTestTasks)
                task.IsDependentOn(preTestTask.Name);

            IEnumerable<RegisteredTask> testTasks = envTasks
                .Where(t => t.AttributeType == typeof(PipelineTaskAttribute) && t.CoreTask == PipelineTask.Test);
            foreach (RegisteredTask testTask in testTasks)
                task.IsDependentOn(testTask.Name);

            IEnumerable<RegisteredTask> postTestTasks = envTasks
                .Where(t => t.AttributeType == typeof(AfterPipelineTaskAttribute) && t.CoreTask == PipelineTask.Test);
            foreach (RegisteredTask postTestTask in postTestTasks)
                task.IsDependentOn(postTestTask.Name);
        }

        /// <summary>
        ///     Registers the CI task that runs both the Build and Test tasks.
        /// </summary>
        private void RegisterCiTask()
        {
            RegisterTask(TaskNames.Ci)
                .Description("Performs CI (Build and test)")
                .IsDependentOn(TaskNames.Test);
        }

        private void RegisterCicdTask(IReadOnlyList<RegisteredTask> envTasks)
        {
            CakeTaskBuilder task = RegisterTask(TaskNames.CiCd)
                .Description("Performs CI/CD (Build, test and deploy)")
                .IsDependentOn("CI");

            IEnumerable<RegisteredTask> preDeployTasks = envTasks
                .Where(t => t.AttributeType == typeof(BeforePipelineTaskAttribute) && t.CoreTask == PipelineTask.Deploy);
            foreach (RegisteredTask preDeployTask in preDeployTasks)
                task.IsDependentOn(preDeployTask.Name);

            IEnumerable<RegisteredTask> deployTasks = envTasks
                .Where(t => t.AttributeType == typeof(PipelineTaskAttribute) && t.CoreTask == PipelineTask.Deploy);
            foreach (RegisteredTask deployTask in deployTasks)
                task.IsDependentOn(deployTask.Name);

            IEnumerable<RegisteredTask> postDeployTasks = envTasks
                .Where(t => t.AttributeType == typeof(AfterPipelineTaskAttribute) && t.CoreTask == PipelineTask.Deploy);
            foreach (RegisteredTask postDeployTask in postDeployTasks)
                task.IsDependentOn(postDeployTask.Name);
        }

        private void RegisterIntegrationTestTask(IReadOnlyList<RegisteredTask> envTasks)
        {
            CakeTaskBuilder task = RegisterTask(TaskNames.IntegrationTest)
                .Description("Performs integration tests")
                .IsDependentOn("Build");

            IEnumerable<RegisteredTask> preIntegrationTestTasks = envTasks
                .Where(t => t.AttributeType == typeof(BeforePipelineTaskAttribute) && t.CoreTask == PipelineTask.IntegrationTest);
            foreach (RegisteredTask preIntegrationTestTask in preIntegrationTestTasks)
                task.IsDependentOn(preIntegrationTestTask.Name);

            IEnumerable<RegisteredTask> integrationTestTasks = envTasks
                .Where(t => t.AttributeType == typeof(PipelineTaskAttribute) && t.CoreTask == PipelineTask.IntegrationTest);
            foreach (RegisteredTask integrationTestTask in integrationTestTasks)
                task.IsDependentOn(integrationTestTask.Name);

            IEnumerable<RegisteredTask> postIntegrationTestTasks = envTasks
                .Where(t => t.AttributeType == typeof(AfterPipelineTaskAttribute) && t.CoreTask == PipelineTask.IntegrationTest);
            foreach (RegisteredTask postIntegrationTestTask in postIntegrationTestTasks)
                task.IsDependentOn(postIntegrationTestTask.Name);
        }

        private IReadOnlyList<RegisteredTask> GetTasksForCiEnvironment()
        {
            string ciEnv = Context.Arguments.GetArgument("ci");
            if (!string.IsNullOrEmpty(ciEnv))
                Log.Information($"CI Environment specified: {ciEnv}");

            return string.IsNullOrEmpty(ciEnv)
                ? _registeredTasks.Where(rt => rt.CiSystem is null).ToList()
                : _registeredTasks.Where(rt => rt.CiSystem is null || rt.CiSystem.Equals(ciEnv, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        internal static class TaskNames
        {
            internal const string Config = "_Config";
            internal const string ListConfigs = "List-Configs";
            internal const string ListEnvs = "List-Envs";
            internal const string ListTasks = "List-Tasks";

            internal const string Build = nameof(Build);
            internal const string Test = nameof(Test);
            internal const string Ci = "CI";
            internal const string CiCd = "CICD";
            internal const string IntegrationTest = nameof(IntegrationTest);
        }
    }
}
