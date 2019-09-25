namespace Cake.Tasks.Config
{
    public sealed class SonarConfig : PluginConfig
    {
        public SonarConfig(TaskConfig taskConfig)
            : base(taskConfig)
        {
        }

        public FuncOrValue<string> Url
        {
            get => Get<FuncOrValue<string>>(Keys.Url);
            set => Set(Keys.Url, value);
        }

        public static class Keys
        {
            public const string Url = "Sonar.Url";
        }
    }
}
