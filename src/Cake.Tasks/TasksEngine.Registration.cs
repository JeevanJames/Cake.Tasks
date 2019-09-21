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

        private void RegisterPlugins()
        {
            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;

            _addinsPath = Path.GetFullPath(Context.Configuration.GetValue("Paths_AddIns"));
            Log.Verbose($"Searching addins path: {_addinsPath}");

            RegisterConfiguration();
            FindPluginPackages();
        }

        private void RegisterConfiguration()
        {
            RegisterSetupAction(_ => new TaskConfig());
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

                    string env = taskAttribute.Environment ?? "<No Env>";
                    switch (taskAttribute)
                    {
                        case CoreTaskAttribute attr:
                            Log.Verbose($"[Core Task] {attr.CoreTaskName} ({env})");
                            var taskAction = (Action<ICakeContext>)method.CreateDelegate(typeof(Action<ICakeContext>));
                            RegisterTask(attr.CoreTaskName)
                                .Does(taskAction);
                            break;
                        case PreTaskAttribute attr:
                            Log.Verbose($"[Pre Task]  {attr.CoreTaskName} - {attr.Name} ({env})");
                            var preTaskAction = (Action<ICakeContext>)method.CreateDelegate(typeof(Action<ICakeContext>));
                            RegisterTask($"Before{attr.CoreTaskName}_{attr.Name}").Does(preTaskAction);
                            break;
                        case PostTaskAttribute attr:
                            Log.Verbose($"[Post Task] {attr.CoreTaskName} - {attr.Name} ({env})");
                            var postTaskAction = (Action<ICakeContext>)method.CreateDelegate(typeof(Action<ICakeContext>));
                            RegisterTask($"After{attr.CoreTaskName}_{attr.Name}").Does(postTaskAction);
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
    }
}
