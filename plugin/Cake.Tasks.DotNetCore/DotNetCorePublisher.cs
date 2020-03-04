﻿#region --- License & Copyright Notice ---
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

using System;
using System.Collections.Generic;
using System.IO;

namespace Cake.Tasks.Config
{
    public class DotNetCorePublisher : Publisher
    {
        public DotNetCorePublisher(string name, string projectFile)
            : base(name)
        {
            if (string.IsNullOrWhiteSpace(projectFile))
                throw new ArgumentException("Specify a valid .NET Core solution or project file to publish.", nameof(projectFile));

            ProjectFile = Path.GetFullPath(projectFile);
            if (!File.Exists(ProjectFile))
                throw new FileNotFoundException($"Cannot find the specified .NET Core solution or project file.", ProjectFile);
        }

        public string ProjectFile { get; }
    }

    public sealed class NuGetPublisher : Publisher
    {
        public NuGetPublisher(string name, string projectFile)
            : base(name)
        {
            if (string.IsNullOrWhiteSpace(projectFile))
                throw new ArgumentException("Specify a valid .NET Core solution or project file to publish.", nameof(projectFile));

            ProjectFile = Path.GetFullPath(projectFile);
            if (!File.Exists(ProjectFile))
                throw new FileNotFoundException($"Cannot find the specified .NET Core solution or project file.", ProjectFile);
        }

        public string ProjectFile { get; set; }

        /// <summary>
        ///     Gets or sets the function to return the NuGet source URL, based on the specified branch
        ///     name.
        /// </summary>
        public Func<string, string> Source { get; set; }

        /// <summary>
        ///     Gets or sets the function to return the NuGet API key, based on the specified branch
        ///     name.
        /// </summary>
        public Func<string, string> ApiKey { get; set; }
    }

    public static class PublisherExtensions
    {
        public static Publisher AddDotNetCore(this IList<Publisher> publishers, string name, string projectFile)
        {
            var publisher = new DotNetCorePublisher(name, projectFile);
            publishers.Add(publisher);
            return publisher;
        }

        public static Publisher AddNuGetPackage(this IList<Publisher> publishers, string name, string projectFile,
            string source = "https: //api.nuget.org/v3/index.json",
            string apiKey = null)
        {
            var publisher = new NuGetPublisher(name, projectFile)
            {
                Source = _ => source,
                ApiKey = _ => apiKey,
            };
            publishers.Add(publisher);
            return publisher;
        }

        public static Publisher AddNuGetPackage(this IList<Publisher> publishers, string name, string projectFile,
            Func<string, string> sourceFn = null,
            Func<string, string> apiKeyFn = null)
        {
            if (sourceFn is null)
                sourceFn = _ => "https: //api.nuget.org/v3/index.json";

            var publisher = new NuGetPublisher(name, projectFile)
            {
                Source = sourceFn,
                ApiKey = apiKeyFn,
            };
            publishers.Add(publisher);
            return publisher;
        }
    }
}
