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

        /// <summary>
        ///     Gets or sets the working directory.
        /// </summary>
        public string WorkingDirectory
        {
            get => Get<string>(Keys.WorkingDirectory);
            set => Set(Keys.WorkingDirectory, value);
        }

        public static class Keys
        {
            public const string Configuration = "Env_Configuration";
            public const string IsCi = "Env_IsCi";
            public const string WorkingDirectory = "Env_WorkingDirectory";
        }
    }
}
