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
using Cake.Core.Diagnostics;

namespace Cake.Tasks.Module.PluginLoaders
{
    public sealed class ProductionPluginLoader : PluginLoader
    {
        public ProductionPluginLoader(string pluginsDir, ICakeLog log)
            : base(pluginsDir, log)
        {
        }

        internal override IEnumerable<RegisteredTask> LoadPlugins()
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
                {
                    IEnumerable<RegisteredTask> dllPlugins = FindPlugins(dllFile);
                    foreach (var dllPlugin in dllPlugins)
                        yield return dllPlugin;
                }
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
}
