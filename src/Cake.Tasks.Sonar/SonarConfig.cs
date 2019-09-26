namespace Cake.Tasks.Config
{
    public sealed class SonarConfig : PluginConfig
    {
        public SonarConfig(TaskConfig taskConfig)
            : base(taskConfig)
        {
        }

        public FuncOrValue<string> Key
        {
            get => Get<FuncOrValue<string>>(Keys.Key);
            set => Set(Keys.Key, value);
        }

        public FuncOrValue<string> Login
        {
            get => Get<FuncOrValue<string>>(Keys.Login);
            set => Set(Keys.Login, value);
        }

        public FuncOrValue<string> OpenCoverReportsPath
        {
            get => Get<FuncOrValue<string>>(Keys.OpenCoverReportsPath);
            set => Set(Keys.OpenCoverReportsPath, value);
        }

        public FuncOrValue<string> TestReportPaths
        {
            get => Get<FuncOrValue<string>>(Keys.TestReportPaths);
            set => Set(Keys.TestReportPaths, value);
        }

        public FuncOrValue<string> Url
        {
            get => Get<FuncOrValue<string>>(Keys.Url);
            set => Set(Keys.Url, value);
        }

        public static class Keys
        {
            public const string Key = "Sonar.Key";
            public const string Login = "Sonar.Login";
            public const string OpenCoverReportsPath = "Sonar.OpenCoverReportsPath";
            public const string TestReportPaths = "Sonar.TestReportPaths";
            public const string Url = "Sonar.Url";
        }
    }
}
