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
