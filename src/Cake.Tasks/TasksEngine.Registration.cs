#pragma warning disable S3885 // "Assembly.Load" should be used

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
            Log.Information($"Searching addins path: {_addinsPath}");

            string[] cakeTasksDirs = Directory.GetDirectories(_addinsPath, "Cake.Tasks.*", SearchOption.TopDirectoryOnly);
            foreach (string cakeTasksDir in cakeTasksDirs)
            {
                Log.Information($"  Found plugin {Path.GetFileName(cakeTasksDir)}");

                string dllDir = Path.Combine(cakeTasksDir, "lib", "netstandard2.0");
                string[] dllFiles = Directory.GetFiles(dllDir, "*.dll", SearchOption.TopDirectoryOnly);

                foreach (string dllFile in dllFiles)
                {
                    FindPlugins(dllFile);
                }
            }
        }

        private void FindPlugins(string dllFile)
        {
            Assembly assembly = Assembly.LoadFile(dllFile);
            IEnumerable<TaskPluginAttribute> taskPlugins = assembly.GetCustomAttributes<TaskPluginAttribute>();
            foreach (TaskPluginAttribute taskPlugin in taskPlugins)
            {
                Log.Information($"    Found plugin");
                Type taskPluginType = taskPlugin.PluginType;
                MethodInfo[] methods = taskPluginType.GetMethods(BindingFlags.Static | BindingFlags.Instance);
                foreach (MethodInfo method in methods)
                {
                    TaskAttribute taskAttribute = method.GetCustomAttribute<TaskAttribute>(inherit: true);
                    if (taskAttribute is null)
                        continue;

                    string env = taskAttribute.Environment ?? "<No Env>";
                    switch (taskAttribute)
                    {
                        case CoreTaskAttribute attr:
                            Log.Information($"    Core Task {attr.CoreTaskName} ({env})");
                            break;
                        case PreTaskAttribute attr:
                            Log.Information($"    Pre Task {attr.CoreTaskName} - {attr.Name} ({env})");
                            break;
                        case PostTaskAttribute attr:
                            Log.Information($"    Post Task {attr.CoreTaskName} - {attr.Name} ({env})");
                            break;
                        case SetupAttribute attr:
                            Log.Information($"    Setup ({env})");
                            break;
                        case TearDownAttribute attr:
                            Log.Information($"    Tear down ({env})");
                            break;
                    }
                }
            }
        }

        private Assembly ResolveAssembly(object sender, ResolveEventArgs args)
        {
            var assemblyName = new AssemblyName(args.Name);
            string assemblyPath = Directory
                .GetFiles(_addinsPath, $"{assemblyName.Name}.dll", SearchOption.AllDirectories)
                .FirstOrDefault();
            Log.Information($"    Looking for {assemblyName.Name}.dll");
            return Assembly.LoadFile(assemblyPath);
        }
    }
}
