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

namespace Cake.Tasks.Config
{
    public abstract class PublishProfile
    {
        protected PublishProfile(string name, string projectFile)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("message", nameof(name));
            if (string.IsNullOrWhiteSpace(projectFile))
                throw new ArgumentException("message", nameof(projectFile));

            Name = name;
            ProjectFile = projectFile;
        }

        public string Name { get; }

        public string ProjectFile { get; }
    }

    public sealed class AspNetPublishProfile : PublishProfile
    {
        public AspNetPublishProfile(string name, string projectFile)
            : base(name, projectFile)
        {
        }
    }

    public sealed class NuGetPackagePublishProfile : PublishProfile
    {
        public NuGetPackagePublishProfile(string name, string projectFile)
            : base(name, projectFile)
        {
        }

        public string Source { get; set; }

        public string ApiKey { get; set; }
    }
}
