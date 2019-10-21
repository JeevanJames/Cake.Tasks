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

namespace Cake.Tasks.Config
{
    public sealed class DotNetCoreConfig : PluginConfig
    {
        public DotNetCoreConfig(TaskConfig taskConfig)
            : base(taskConfig)
        {
            Build = new BuildConfig(taskConfig);
            Test = new TestConfig(taskConfig);
        }

        public BuildConfig Build { get; }

        public TestConfig Test { get; }

        public sealed class BuildConfig : PluginConfig
        {
            public BuildConfig(TaskConfig taskConfig)
                : base(taskConfig)
            {
            }

            public ConfigValue<string> ProjectFile
            {
                get => Get<ConfigValue<string>>(Keys.BuildProjectFile);
                set => Set(Keys.BuildProjectFile, value);
            }
        }

        public sealed class TestConfig : PluginConfig
        {
            public TestConfig(TaskConfig taskConfig)
                : base(taskConfig)
            {
            }

            public ConfigValue<bool> Skip
            {
                get => Get<ConfigValue<bool>>(Keys.TestSkip);
                set => Set(Keys.TestSkip, value);
            }

            public ConfigValue<string> ProjectFile
            {
                get => Get<ConfigValue<string>>(Keys.TestProjectFile);
                set => Set(Keys.TestProjectFile, value);
            }

            public ConfigValue<string> Filter
            {
                get => Get<ConfigValue<string>>(Keys.TestFilter);
                set => Set(Keys.TestFilter, value);
            }
        }

        public static class Keys
        {
            public const string BuildProjectFile = "DotNetCore_Build_ProjectFile";

            public const string TestSkip = "DotNetCore_Test_Skip";
            public const string TestProjectFile = "DotNetCore_Test_ProjectFile";
            public const string TestFilter = "DotNetCore_Test_Filter";
        }
    }

    public abstract class DotNetCorePublishProfile : PublishProfile
    {
        protected DotNetCorePublishProfile(string name, string projectFile)
            : base(name, projectFile)
        {
        }
    }

    public sealed class AspNetPublishProfile : DotNetCorePublishProfile
    {
        public AspNetPublishProfile(string name, string projectFile)
            : base(name, projectFile)
        {
        }
    }

    public sealed class NuGetPackagePublishProfile : DotNetCorePublishProfile
    {
        public NuGetPackagePublishProfile(string name, string projectFile)
            : base(name, projectFile)
        {
        }

        public string Source { get; set; }

        public string ApiKey { get; set; }
    }
}
