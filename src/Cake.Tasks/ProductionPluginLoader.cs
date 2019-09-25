#pragma warning disable S3885 // "Assembly.Load" should be used

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cake.Core.Diagnostics;

namespace Cake.Tasks.Module
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
