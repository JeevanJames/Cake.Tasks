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
    public sealed class EnvConfig : PluginConfig
    {
        public EnvConfig(TaskConfig taskConfig)
            : base(taskConfig)
        {
            Directories = new DirectoryConfig(taskConfig);
        }

        public string Configuration
        {
            get => Get<string>(Keys.Configuration);
            set => Set(Keys.Configuration, value);
        }

        /// <summary>
        ///     Gets or sets a value indicating whether Cake is running in a CI environment.
        /// </summary>
        public bool IsCi
        {
            get => Get<bool>(Keys.IsCi);
            set => Set(Keys.IsCi, value);
        }

        public FuncOrValue<int> BuildNumber
        {
            get => GetFuncOrValue<int>(Keys.BuildNumber);
            set => Set(Keys.BuildNumber, value);
        }

        public FuncOrValue<string> Version
        {
            get => GetFuncOrValue<string>(Keys.Version);
            set => Set(Keys.Version, value);
        }

        public FuncOrValue<string> FullVersion
        {
            get => GetFuncOrValue<string>(Keys.FullVersion);
            set => Set(Keys.FullVersion, value);
        }

        public FuncOrValue<string> BuildVersion
        {
            get => GetFuncOrValue<string>(Keys.BuildVersion);
            set => Set(Keys.BuildVersion, value);
        }

        public DirectoryConfig Directories { get; }

        public sealed class DirectoryConfig : PluginConfig
        {
            public DirectoryConfig(TaskConfig taskConfig)
                : base(taskConfig)
            {
            }

            /// <summary>
            ///     Gets or sets the working directory.
            /// </summary>
            public string Working
            {
                get => Get<string>(Keys.WorkingDirectory);
                set => Set(Keys.WorkingDirectory, value);
            }

            public string Artifacts
            {
                get => Get<string>(Keys.ArtifactsDirectory);
                set => Set(Keys.ArtifactsDirectory, value);
            }

            public string BuildOutput
            {
                get => Get<string>(Keys.BuildOutputDirectory);
                set => Set(Keys.BuildOutputDirectory, value);
            }

            public string TestOutput
            {
                get => Get<string>(Keys.TestOutputDirectory);
                set => Set(Keys.TestOutputDirectory, value);
            }
        }

        public static class Keys
        {
            public const string Configuration = "Env_Configuration";
            public const string IsCi = "Env_IsCi";

            public const string BuildNumber = "Env_BuildNumber";
            public const string Version = "Env_Version";
            public const string FullVersion = "Env_FullVersion";
            public const string BuildVersion = "Env_BuildVersion";

            public const string WorkingDirectory = "Env_Directory_Working";
            public const string ArtifactsDirectory = "Env_Directory_Artifacts";
            public const string BuildOutputDirectory = "Env_Directory_BuildOutput";
            public const string TestOutputDirectory = "Env_Directory_TestOutput";
        }
    }
}
