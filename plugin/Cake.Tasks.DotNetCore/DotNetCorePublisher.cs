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

using System;
using System.Collections.Generic;
using System.IO;

namespace Cake.Tasks.Config
{
    public abstract class DotNetCorePublisher : Publisher
    {
        protected DotNetCorePublisher(string projectFile)
        {
            if (string.IsNullOrWhiteSpace(projectFile))
                throw new ArgumentException("Specify a valid .NET Core solution or project file to publish.", nameof(projectFile));

            ProjectFile = Path.GetFullPath(projectFile);
            if (!File.Exists(ProjectFile))
                throw new FileNotFoundException($"Cannot find the specified .NET Core solution or project file '{ProjectFile}'.", ProjectFile);
        }

        public string ProjectFile { get; }
    }

    public class AspNetCorePublisher : DotNetCorePublisher
    {
        public AspNetCorePublisher(string projectFile)
            : base(projectFile)
        {
        }
    }

    public class NuGetPublisher : DotNetCorePublisher
    {
        public NuGetPublisher(string projectFile)
            : base(projectFile)
        {
        }

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

        /// <summary>
        ///     Gets or sets a value indicating whether to publish in the new SNupkg package format.
        /// </summary>
        public bool PublishAsSnupkg { get; set; }
    }

    public static class PublisherExtensions
    {
        public static Publisher AddAspNetCore(this IList<Publisher> publishers, string projectFile)
        {
            var publisher = new AspNetCorePublisher(projectFile);
            publishers.Add(publisher);
            return publisher;
        }

        [Obsolete("Deprecated in favor of overload without the name parameter")]
        public static Publisher AddAspNetCore(this IList<Publisher> publishers, string name, string projectFile)
        {
            var publisher = new AspNetCorePublisher(projectFile);
            publishers.Add(publisher);
            return publisher;
        }

        /// <summary>
        ///     Publishes the specified project as a NuGet package.
        /// </summary>
        /// <param name="publishers">The publishers collection.</param>
        /// <param name="projectFile">The .NET project file to publish.</param>
        /// <param name="source">The NuGet source feed URL to publish to.</param>
        /// <param name="apiKey">Optional API key to the NuGet source feed.</param>
        /// <param name="publishAsSnupkg">True to publish the package using the new SNUPKG format.</param>
        /// <returns>The instance of the created <see cref="Publisher"/> class.</returns>
        public static Publisher AddNuGetPackage(this IList<Publisher> publishers, string projectFile,
            string source = null,
            string apiKey = null,
            bool publishAsSnupkg = false)
        {
            var publisher = new NuGetPublisher(projectFile)
            {
                PublishAsSnupkg = publishAsSnupkg,
            };
            if (source != null)
                publisher.Source = _ => source;
            if (apiKey != null)
                publisher.ApiKey = _ => apiKey;
            publishers.Add(publisher);
            return publisher;
        }

        public static Publisher AddNuGetPackage(this IList<Publisher> publishers, string projectFile,
            Func<string, string> sourceFn = null,
            Func<string, string> apiKeyFn = null,
            bool publishAsSnupkg = false)
        {
            var publisher = new NuGetPublisher(projectFile)
            {
                Source = sourceFn,
                ApiKey = apiKeyFn,
                PublishAsSnupkg = publishAsSnupkg,
            };
            publishers.Add(publisher);
            return publisher;
        }
    }
}
