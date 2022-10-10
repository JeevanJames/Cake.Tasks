// Cake Tasks framework for Cake Build
// Copyright (c) 2019-2022 Jeevan James
// This file is licensed to you under the Apache License, Version 2.0.
// See the LICENSE file in the project root for more information.

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
using Cake.Tasks.Module.Internal;
using Cake.Tasks.Module.PluginLoaders;

namespace Cake.Tasks.Module;

public sealed partial class TasksEngine
{
    private List<RegisteredTask> _registeredTasks;

    private void InitializeCakeTasksSystem()
    {
        RegisterSetupTaskAndInitializeEnvConfig();
        DiscoverPluginTasks();
        RegisterPluginTasks();
        RegisterBuiltInTasks();
        RegisterPipelineTasks();
    }

    /// <summary>
    ///     Registers the setup task. This involves initializing the <see cref="EnvConfig"/> configuration
    ///     and logging the add-in directories for debugging purposes.
    /// </summary>
    private void RegisterSetupTaskAndInitializeEnvConfig()
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

            env.Directories.Working = ctx.Environment.WorkingDirectory.FullPath;
            string outputDirectory = Path.Combine(env.Directories.Working, ".ci");
            env.Directories.Artifacts = Path.Combine(outputDirectory, "artifacts");
            env.Directories.BinaryOutput = Path.Combine(outputDirectory, "binaries");
            env.Directories.PublishOutput = Path.Combine(outputDirectory, "publish");
            env.Directories.TestOutput = Path.Combine(outputDirectory, "testresults");

            env.Repository.Name = env.Name;
            env.Repository.Url = null;
            env.Repository.Type = null;
            env.Repository.Branch = null;
            env.Repository.Commit = null;

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

            if (registeredTask is RegisteredRegularTask { RequiresConfig: true })
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
        // the final values.
        void RegisterListConfigsTask()
        {
            CakeTaskBuilder listConfigsTask = RegisterTask(TaskNames.ListConfigs)
                .Description("Lists all available configurations.");
            foreach (RegisteredConfigTask configTask in GetTasksForCiEnvironment().OfType<RegisteredConfigTask>())
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
    ///             <term>Package</term>
    ///             <description>Creates and publishes packages, like NuGet, NPM, Docker images, etc.</description>
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
    ///             <term>CIPackage</term>
    ///             <description>Runs CI + Package</description>
    ///         <item>
    ///         </item>
    ///         <item>
    ///             <term>CICD</term>
    ///             <description>Runs CIPackage + Deploy</description>
    ///         </item>
    ///     </list>
    /// </summary>
    private void RegisterPipelineTasks()
    {
        IReadOnlyList<RegisteredTask> envTasks = GetTasksForCiEnvironment();

        RegisterConfigTask(envTasks);
        RegisterPipelineItemTask(TaskNames.Build, "Builds the code.", PipelineTask.Build,
            dependentOn: TaskNames.Config, envTasks);
        RegisterPipelineItemTask(TaskNames.Test, "Runs unit tests or fast integration tests.", PipelineTask.Test,
            dependentOn: TaskNames.Build, envTasks);
        RegisterTask(TaskNames.Ci)
            .Description("Performs CI (Build and test)")
            .IsDependentOn(TaskNames.Test);
        RegisterPipelineItemTask(TaskNames.CiPackage, "Performs CI & Packaging (Build, test and package)", PipelineTask.Package,
            dependentOn: TaskNames.Ci, envTasks);
        RegisterPipelineItemTask(TaskNames.CiCd, "Performs CI/CD (Build, test, package and deploy)", PipelineTask.Deploy,
            dependentOn: TaskNames.CiPackage, envTasks);
        RegisterPipelineItemTask(TaskNames.CiIntegrationTest, "Runs the full suite of integration tests.",
            PipelineTask.IntegrationTest, dependentOn: TaskNames.Build, envTasks);
        RegisterTeardownTask(envTasks);
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
            .OfType<RegisteredConfigTask>()
            .OrderBy(t => t.Order);

        foreach (RegisteredTask configTask in configTasks)
            task.IsDependentOn(configTask.Name);

        task.Does(ctx =>
        {
            TaskConfig config = TaskConfig.Current;

            LoadVariables(config, ctx.Environment.WorkingDirectory.FullPath);

            // Run configurations specified in the build.cake file
            config.PerformDeferredSetup();

            // Override configurations from environment variables
            foreach (KeyValuePair<string, string> envVar in ctx.Environment.GetEnvironmentVariables())
            {
                if (config.Data.ContainsKey(envVar.Key))
                    config.Data[envVar.Key] = envVar.Value;
            }

            // Override configurations from command line arguments
            foreach (string key in config.Data.Keys.ToList())
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

        static void LoadVariables(TaskConfig config, string directory)
        {
            string variableFilePath = Path.Combine(directory, "ci.vars");
            if (!File.Exists(variableFilePath))
                return;

            StreamReader reader = File.OpenText(variableFilePath);
            string line = reader.ReadLine();
            while (line is not null)
            {
                LoadVariable(config, line);
                line = reader.ReadLine();
            }
        }

        static void LoadVariable(TaskConfig config, string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return;

            string trimmedLine = line.Trim();
            if (trimmedLine.StartsWith("#"))
                return;

            string[] parts = trimmedLine.Split(new[] { '=' }, 2, StringSplitOptions.None);
            if (parts.Length != 2)
                return;

            string key = parts[0].Trim();
            string value = parts[1].Trim();
            if (bool.TryParse(value, out bool boolValue))
                AddOrUpdateConfig(config, key, boolValue);
            else if (double.TryParse(value, out double doubleValue))
                AddOrUpdateConfig(config, key, doubleValue);
            else if (long.TryParse(value, out long longValue))
                AddOrUpdateConfig(config, key, longValue);
            else
                AddOrUpdateConfig(config, key, value.Trim('"'));
        }

        static void AddOrUpdateConfig<T>(TaskConfig config, string key, T value)
        {
            if (config.Data.ContainsKey(key))
                config.Data[key] = value;
            else
                config.Data.Add(key, value);
        }
    }

    private void RegisterPipelineItemTask(string name, string description, PipelineTask coreTask, string dependentOn,
        IReadOnlyList<RegisteredTask> envTasks)
    {
        CakeTaskBuilder task = RegisterTask(name)
            .Description(description)
            .IsDependentOn(dependentOn);

        // Add pre tasks.
        IEnumerable<RegisteredTask> preTasks = envTasks
            .OfType<RegisteredBeforeAfterPipelineTask>()
            .Where(t => t.CoreTask == coreTask && t.EventType == TaskEventType.BeforeTask)
            .OrderBy(t => t.Order);
        foreach (RegisteredTask preTask in preTasks)
            task.IsDependentOn(preTask.Name);

        // Add core tasks.
        IEnumerable<RegisteredTask> mainTasks = envTasks
            .OfType<RegisteredPipelineTask>()
            .Where(t => t.CoreTask == coreTask);
        foreach (RegisteredTask mainTask in mainTasks)
            task.IsDependentOn(mainTask.Name);

        // Add post tasks.
        IEnumerable<RegisteredTask> postTasks = envTasks
            .OfType<RegisteredBeforeAfterPipelineTask>()
            .Where(t => t.CoreTask == coreTask && t.EventType == TaskEventType.AfterTask)
            .OrderBy(t => t.Order);
        foreach (RegisteredTask postTask in postTasks)
            task.IsDependentOn(postTask.Name);
    }

    private void RegisterTeardownTask(IReadOnlyList<RegisteredTask> envTasks)
    {
        RegisterTeardownAction<TaskConfig>((ctx, cfg) =>
        {
            IEnumerable<RegisteredTeardownTask> teardownTasks = envTasks
                .OfType<RegisteredTeardownTask>()
                .OrderBy(t => t.Order);

            foreach (RegisteredTeardownTask teardownTask in teardownTasks)
            {
                teardownTask.Method.Invoke(null, new object[] { ctx, cfg });
            }
        });
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
        internal const string CiPackage = "CIPackage";
        internal const string CiCd = "CICD";
        internal const string CiIntegrationTest = "CIIntegrationTest";
    }
}
