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

namespace Cake.Tasks.Module.PluginLoaders;

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
            foreach (RegisteredTask dllPlugin in FindPlugins(dllFile))
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
