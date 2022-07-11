// Cake Tasks framework for Cake Build
// Copyright (c) 2019-2022 Jeevan James
// This file is licensed to you under the Apache License, Version 2.0.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using Semver;

namespace Cake.Tasks.Core.Internal
{
    public static class AddinFinder
    {
        public static IEnumerable<string> Find(string addinsBaseDir)
        {
            return Directory
                .EnumerateDirectories(addinsBaseDir, "Cake.Tasks.*", SearchOption.TopDirectoryOnly)
                .Select(dir =>
                {
                    Match versionMatch = VersionPattern.Match(dir);
                    return (path: dir,
                        name: dir.Substring(0, versionMatch.Index - 1),
                        version: SemVersion.Parse(versionMatch.Value));
                })
                .ToLookup(addin => addin.name)
                .Select(addinSet => addinSet.OrderByDescending(addin => addin.version).First().path);
        }

        private static readonly Regex VersionPattern = new Regex(
            @"(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)(?:-((?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\+([0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?$",
            RegexOptions.Compiled);
    }
}
