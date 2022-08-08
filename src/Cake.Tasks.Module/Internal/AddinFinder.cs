// Cake Tasks framework for Cake Build
// Copyright (c) 2019-2022 Jeevan James
// This file is licensed to you under the Apache License, Version 2.0.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using Semver;

namespace Cake.Tasks.Module.Internal;

internal static class AddinFinder
{
    /// <summary>
    ///     Finds all add-in directories given a base directory for add-ins.
    ///     <para/>
    ///     This method looks for immediate directories whose names follow NuGet package directory
    ///     naming conventions (<c>{package name}.{semver}</c>) and whose names start with
    ///     <c>Cake.Tasks</c>. If there are multiple directories for the same package, but with
    ///     different versions, then the package directory with the highest version is chosen.
    /// </summary>
    /// <param name="addinsBaseDir">The base directory to search under.</param>
    /// <returns>Collection of directories containing Cake.Task add-ins.</returns>
    internal static IEnumerable<string> Find(string addinsBaseDir)
    {
        return Directory
            .EnumerateDirectories(addinsBaseDir, "Cake.Tasks.*", SearchOption.TopDirectoryOnly)
            .Select(dir =>
            {
                Match versionMatch = VersionPattern.Match(dir);
                return (Path: dir,
                    Name: dir.Substring(0, versionMatch.Index - 1),
                    Version: SemVersion.Parse(versionMatch.Value));
            })
            .ToLookup(addin => addin.Name)
            .Select(addinSet => addinSet.OrderByDescending(addin => addin.Version).First().Path);
    }

    private static readonly Regex VersionPattern = new(
        @"(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)(?:-((?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\+([0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?$",
        RegexOptions.Compiled, TimeSpan.FromSeconds(1));
}
