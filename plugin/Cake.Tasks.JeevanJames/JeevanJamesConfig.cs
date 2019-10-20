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
    public sealed class JeevanJamesConfig : PluginConfig
    {
        public JeevanJamesConfig(TaskConfig taskConfig)
            : base(taskConfig)
        {
        }

        public ConfigList<PublishProfile> PublishProfiles
        {
            get => GetList<PublishProfile>(Keys.PublishProfiles);
            set => Set(Keys.PublishProfiles, value);
        }

        public string Source
        {
            get => Get<string>(Keys.Source);
            set => Set(Keys.Source, value);
        }

        public string ApiKey
        {
            get => Get<string>(Keys.ApiKey);
            set => Set(Keys.ApiKey, value);
        }

        public static class Keys
        {
            public const string PublishProfiles = "JeevanJames_PublishProfiles";
            public const string Source = "JeevanJames_NuGetSource";
            public const string ApiKey = "JeevanJames_NuGetApiKey";
        }
    }
}
