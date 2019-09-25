#pragma warning disable S3885 // "Assembly.Load" should be used

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using Cake.Core.Diagnostics;

namespace Cake.Tasks.Module
{
    public sealed class DebugPluginLoader : PluginLoader
    {
        public DebugPluginLoader(string pluginsDir, ICakeLog log)
            : base(pluginsDir, log)
        {
        }

        internal override IEnumerable<RegisteredTask> LoadPlugins()
        {
            string[] dllFiles = Directory.GetFiles(PluginsDir, "Cake.Tasks.*.dll", SearchOption.TopDirectoryOnly);

            foreach (string dllFile in dllFiles)
            {
                IEnumerable<RegisteredTask> dllPlugins = FindPlugins(dllFile);
                foreach (var dllPlugin in dllPlugins)
                    yield return dllPlugin;
            }
        }

        protected override Assembly ResolveAssembly(object sender, ResolveEventArgs args)
        {
            var assemblyName = new AssemblyName(args.Name);
            string assemblyPath = Directory
                .GetFiles(PluginsDir, $"{assemblyName.Name}.dll", SearchOption.TopDirectoryOnly)
                .FirstOrDefault();
            Log.Verbose($"[Assembly Lookup] {assemblyName.Name}.dll");
            return Assembly.LoadFile(assemblyPath);
        }
    }
}
