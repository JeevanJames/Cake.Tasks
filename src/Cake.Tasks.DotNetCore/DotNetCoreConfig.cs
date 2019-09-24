using Cake.Tasks.Core;

namespace Cake.Tasks.DotNetCore
{
    public sealed class DotNetCoreConfig : PluginConfig
    {
        public DotNetCoreConfig(TaskConfig taskConfig)
            : base(taskConfig)
        {
        }

        public FuncOrValue<string> BuildProjectFile
        {
            get => Get<FuncOrValue<string>>("DotNetCore.BuildProjectFiles");
            set => Set("DotNetCore.BuildProjectFiles", value);
        }
    }
}
