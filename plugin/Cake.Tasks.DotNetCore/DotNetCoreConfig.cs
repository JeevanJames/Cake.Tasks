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

namespace Cake.Tasks.Config
{
    public sealed class DotNetCoreConfig : PluginConfig
    {
        public DotNetCoreConfig(TaskConfig taskConfig)
            : base(taskConfig)
        {
            Build = new BuildConfig(taskConfig);
            Test = new TestConfig(taskConfig);
            Coverage = new CoverageConfig(taskConfig);
            NuGetPublisher = new NuGetPublisherConfig(taskConfig);
        }

        public BuildConfig Build { get; }

        public TestConfig Test { get; }

        public CoverageConfig Coverage { get; }

        public NuGetPublisherConfig NuGetPublisher { get; }

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

            public ConfigValue<string> Logger
            {
                get => GetValue<string>(Keys.TestLogger);
                set => Set(Keys.TestLogger, value);
            }
        }

        public sealed class CoverageConfig : PluginConfig
        {
            public CoverageConfig(TaskConfig taskConfig)
                : base(taskConfig)
            {
            }

            public IList<string> ExcludeFilters
            {
                get => Get(Keys.CoverageExcludeFilters, new List<string>());
                set => Set(Keys.CoverageExcludeFilters, value);
            }

            public IList<string> IncludeFilters
            {
                get => Get(Keys.CoverageIncludeFilters, new List<string>());
                set => Set(Keys.CoverageIncludeFilters, value);
            }
        }

        public sealed class NuGetPublisherConfig : PluginConfig
        {
            public NuGetPublisherConfig(TaskConfig taskConfig)
                : base(taskConfig)
            {
            }

            public Func<string, string> SourceFn
            {
                get => Get<Func<string, string>>(Keys.NuGetPublisherSourceFn);
                set => Set(Keys.NuGetPublisherSourceFn, value);
            }

            public Func<string, string> ApiKeyFn
            {
                get => Get<Func<string, string>>(Keys.NuGetPublisherApiKeyFn);
                set => Set(Keys.NuGetPublisherApiKeyFn, value);
            }
        }

        public static class Keys
        {
            public const string BuildProjectFile = "DotNetCore_Build_ProjectFile";

            public const string TestSkip = "DotNetCore_Test_Skip";
            public const string TestProjectFile = "DotNetCore_Test_ProjectFile";
            public const string TestFilter = "DotNetCore_Test_Filter";
            public const string TestLogger = "DotNetCore_Test_Logger";

            public const string CoverageExcludeFilters = "DotNetCore_Coverage_ExcludeFilters";
            public const string CoverageIncludeFilters = "DotNetCore_Coverage_IncludeFilters";

            public const string NuGetPublisherSourceFn = "DotNetCore_NuGetPublisher_SourceFn";
            public const string NuGetPublisherApiKeyFn = "DotNetCore_NuGetPublisher_ApiKeyFn";
        }
    }
}
