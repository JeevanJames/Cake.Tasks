// Cake Tasks framework for Cake Build
// Copyright (c) 2019-2022 Jeevan James
// This file is licensed to you under the Apache License, Version 2.0.
// See the LICENSE file in the project root for more information.

#pragma warning disable S3885 // "Assembly.Load" should be used

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using Cake.Core.Diagnostics;
using Cake.Tasks.Core.Internal;

namespace Cake.Tasks.Module.PluginLoaders;

public sealed class ProductionPluginLoader : PluginLoader
{
    public ProductionPluginLoader(string pluginsDir, ICakeLog log)
        : base(pluginsDir, log)
    {
    }

    internal override IEnumerable<RegisteredTask> LoadPlugins()
    {
        AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;

        foreach (string cakeTasksDir in AddinFinder.Find(PluginsDir))
        {
            //TODO: Need to support for framework targets
            string dllDir = Path.Combine(cakeTasksDir, "lib", "netstandard2.0");
            if (!Directory.Exists(dllDir))
                continue;

            string[] dllFiles = Directory.GetFiles(dllDir, "*.dll", SearchOption.TopDirectoryOnly);

            foreach (string dllFile in dllFiles)
            {
                foreach (RegisteredTask dllPlugin in FindPlugins(dllFile))
                    yield return dllPlugin;
            }
        }
    }

    protected override Assembly ResolveAssembly(object sender, ResolveEventArgs args)
    {
        AssemblyName assemblyName = new(args.Name);
        string assemblyPath = Directory
            .GetFiles(PluginsDir, $"{assemblyName.Name}.dll", SearchOption.AllDirectories)
            .FirstOrDefault();
        Log.Verbose($"[Assembly Lookup] {assemblyName.Name}.dll");
        return Assembly.LoadFile(assemblyPath);
    }
}
