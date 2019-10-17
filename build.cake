#module "nuget:?package=Cake.Tasks.Module&prerelease"
#tool "nuget:?package=GitVersion.CommandLine"
#addin "nuget:https://myget.org/f/cake-tasks/?package=Cake.Tasks.JeevanJames&prerelease"

ConfigureTask<DotNetCoreConfig>(cfg =>
{
    cfg.Test.ProjectFile = @".\TestLibrary.Tests\TestLibrary.Tests.csproj";
    cfg.Publish.ProjectFile = @".\TestLibrary\TestLibrary.csproj";
});

RunTarget(Argument("target", "Default"));
