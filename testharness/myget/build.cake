#module "nuget:?package=Cake.Tasks.Module&prerelease"

#tool "nuget:?package=GitVersion.CommandLine"

#addin "nuget:?package=Cake.Tasks.JeevanJames&prerelease"
// #addin "nuget:?package=Cake.Tasks.Core&prerelease"
// #addin "nuget:?package=Cake.Tasks.DotNetCore&prerelease"
// #addin "nuget:?package=Cake.Tasks.GitVersion&prerelease"
// #addin "nuget:?package=Cake.Tasks.Local&prerelease"

ConfigureTask<DotNetCoreConfig>(cfg =>
{
    cfg.Test.ProjectFile = @".\TestLibrary.Tests\TestLibrary.Tests.csproj";
    cfg.Publish.ProjectFile = @".\TestLibrary\TestLibrary.csproj";
});

RunTarget(Argument("target", "Default"));
