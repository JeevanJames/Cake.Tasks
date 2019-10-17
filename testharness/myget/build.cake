#module "nuget:?package=Cake.Tasks.Module&prerelease"

#addin "nuget:https://myget.org/f/cake-tasks/?package=Cake.Tasks.JeevanJames&prerelease&loaddependencies=true"

ConfigureTask<DotNetCoreConfig>(cfg =>
{
    cfg.Test.ProjectFile = @".\TestLibrary.Tests\TestLibrary.Tests.csproj";
});

RunTarget(Argument("target", "Default"));
