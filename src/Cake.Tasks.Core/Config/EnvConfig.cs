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

namespace Cake.Tasks.Config
{
    /// <summary>
    ///     Global environment configuration, including CI, directory and version information.
    /// </summary>
    public sealed class EnvConfig : PluginConfig
    {
        public EnvConfig(TaskConfig taskConfig)
            : base(taskConfig)
        {
            Ci = new CiConfig(taskConfig);
            Directories = new DirectoryConfig(taskConfig);
            Version = new VersionConfig(taskConfig);
        }

        /// <summary>
        ///     Gets or sets the build configuration, such as Debug or Release.
        /// </summary>
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

        public IList<PublishProfile> PublishProfiles
        {
            get => Get(Keys.PublishProfiles, new List<PublishProfile>());
            set => Set(Keys.PublishProfiles, value);
        }

        public IList<Publisher> Publishers
        {
            get => Get(Keys.Publishers, new List<Publisher>());
            set => Set(Keys.Publishers, value);
        }

        /// <summary>
        ///     Gets the configuration for the CI settings.
        /// </summary>
        public CiConfig Ci { get; }

        /// <summary>
        ///     Gets the configuration for the location of common directories used during a build.
        /// </summary>
        public DirectoryConfig Directories { get; }

        /// <summary>
        ///     Gets the configuration for version-related information.
        /// </summary>
        public VersionConfig Version { get; }

        public sealed class CiConfig : PluginConfig
        {
            public CiConfig(TaskConfig taskConfig)
                : base(taskConfig)
            {
            }

            public IList<string> Artifacts
            {
                get => Get<List<string>>(Keys.CiArtifacts);
                set => Set(Keys.CiArtifacts, value);
            }

            public IList<string> TestArtifacts
            {
                get => Get<List<string>>(Keys.CiTestArtifacts);
                set => Set(Keys.CiTestArtifacts, value);
            }
        }

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
                get => Get<string>(Keys.DirectoryWorking);
                set => Set(Keys.DirectoryWorking, value);
            }

            /// <summary>
            ///     Gets or sets the directory for storing build artifacts.
            /// </summary>
            public string Artifacts
            {
                get => Get<string>(Keys.DirectoryArtifacts);
                set => Set(Keys.DirectoryArtifacts, value);
            }

            /// <summary>
            ///     Gets or sets the directory for storing binary output from the build process.
            /// </summary>
            public string BinaryOutput
            {
                get => Get<string>(Keys.DirectoryBinaryOutput);
                set => Set(Keys.DirectoryBinaryOutput, value);
            }

            /// <summary>
            ///     Gets or sets the directory for storing test output from the build process.
            /// </summary>
            public string TestOutput
            {
                get => Get<string>(Keys.DirectoryTestOutput);
                set => Set(Keys.DirectoryTestOutput, value);
            }
        }

        public sealed class VersionConfig : PluginConfig
        {
            public VersionConfig(TaskConfig taskConfig)
                : base(taskConfig)
            {
            }

            /// <summary>
            ///     Gets or sets an unique build number for the build execution.
            /// </summary>
            public ConfigValue<int> BuildNumber
            {
                get => GetValue<int>(Keys.VersionBuildNumber);
                set => Set(Keys.VersionBuildNumber, value);
            }

            /// <summary>
            ///     Gets or sets the primary version number for the build. This is in a simple
            ///     {Major}.{Minor}.{Patch} format without prerelease, build or other extra information.
            ///     <para/>
            ///     The value can repeat across multiple builds and is not unique.
            /// </summary>
            public ConfigValue<string> Primary
            {
                get => GetValue<string>(Keys.VersionPrimary);
                set => Set(Keys.VersionPrimary, value);
            }

            /// <summary>
            ///     Gets or sets the full semantic version of the build. This includes prerelease, build
            ///     and other extra information.
            ///     <para/>
            ///     The value will be unique for every new commit, but if multiple builds are run for the
            ///     same commit, it will remain the same, so it cannot be considered a unique build value.
            /// </summary>
            public ConfigValue<string> Full
            {
                get => GetValue<string>(Keys.VersionFull);
                set => Set(Keys.VersionFull, value);
            }

            /// <summary>
            ///     Gets or sets an unique version number for the build.
            ///     <para/>
            ///     The value is unique for every build, regardless of whether a new commit is made or
            ///     the same commit is being rebuilt.
            /// </summary>
            public ConfigValue<string> Build
            {
                get => GetValue<string>(Keys.VersionBuild);
                set => Set(Keys.VersionBuild, value);
            }
        }

        public static class Keys
        {
            public const string Configuration = "Env_Configuration";
            public const string IsCi = "Env_IsCI";

            public const string PublishProfiles = "Env_PublishProfiles";
            public const string Publishers = "Env_Publishers";

            public const string CiArtifacts = "Env_CI_Artifacts";
            public const string CiTestArtifacts = "Env_CI_TestArtifacts";

            public const string DirectoryWorking = "Env_Directory_Working";
            public const string DirectoryArtifacts = "Env_Directory_Artifacts";
            public const string DirectoryBinaryOutput = "Env_Directory_BinaryOutput";
            public const string DirectoryTestOutput = "Env_Directory_TestOutput";

            public const string VersionBuildNumber = "Env_Version_BuildNumber";
            public const string VersionPrimary = "Env_Version_Primary";
            public const string VersionFull = "Env_Version_Full";
            public const string VersionBuild = "Env_Version_Build";
        }
    }
}
