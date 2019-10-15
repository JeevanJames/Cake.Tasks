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

using Cake.Tasks.Config;

namespace Cake.Tasks.Config
{
    public sealed class GitVersionConfig : PluginConfig
    {
        public GitVersionConfig(TaskConfig taskConfig)
            : base(taskConfig)
        {
        }

        public Common.Tools.GitVersion.GitVersion Version
        {
            get => Get<Common.Tools.GitVersion.GitVersion>(Keys.Version);
            set => Set(Keys.Version, value);
        }

        public static class Keys
        {
            public const string Version = "GitVersion_Version";
        }
    }
}