using Cake.Tasks.Core;

namespace Cake.Tasks.DotNetCore
{
    public sealed class DotNetCoreConfig : PluginConfig
    {
        public DotNetCoreConfig(TaskConfig taskConfig)
            : base(taskConfig)
        {
        }

        public TaskConfigValue<string> BuildProjectFile
        {
            get => Get<TaskConfigValue<string>>("DotNetCore.BuildProjectFiles");
            set => Set("DotNetCore.BuildProjectFiles", value);
        }
    }
}
