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
    public sealed class CiConfig : PluginConfig
    {
        public CiConfig(TaskConfig taskConfig)
            : base(taskConfig)
        {
        }

        public int BuildNumber
        {
            get => Get<int>(Keys.BuildNumber);
            set => Set(Keys.BuildNumber, value);
        }

        public FuncOrValue<string> Version
        {
            get => GetFuncOrValue<string>(Keys.Version);
            set => Set(Keys.Version, value);
        }

        public string ArtifactsDirectory
        {
            get => Get<string>(Keys.ArtifactsDirectory);
            set => Set(Keys.ArtifactsDirectory, value);
        }

        public string BuildOutputDirectory
        {
            get => Get<string>(Keys.BuildOutputDirectory);
            set => Set(Keys.BuildOutputDirectory, value);
        }

        public string TestOutputDirectory
        {
            get => Get<string>(Keys.TestOutputDirectory);
            set => Set(Keys.TestOutputDirectory, value);
        }

        public static class Keys
        {
            public const string BuildNumber = "CI_BuildNumber";
            public const string Version = "CI_Version";
            public const string ArtifactsDirectory = "CI_ArtifactsDirectory";
            public const string BuildOutputDirectory = "CI_BuildOutputDirectory";
            public const string TestOutputDirectory = "CI_TestOutputDirectory";
        }
    }
}
