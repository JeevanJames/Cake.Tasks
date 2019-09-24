namespace Cake.Tasks.DotNetCore
{
    public static class Config
    {
        public const string Prefix = "DotNetCore.";

        // Build
        public static readonly string BuildProjectFiles = $"{Prefix}BuildProjectFiles";

        // Test
        public static readonly string TestProjectFiles = $"{Prefix}TestProjectFiles";
    }
}
