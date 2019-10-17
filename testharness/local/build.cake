#tool "nuget:?package=GitVersion.CommandLine"

#r ".\tools\Cake.Tasks.Core.dll"
#r ".\tools\Cake.Tasks.GitVersion.dll"
#r ".\tools\Cake.Tasks.DotNetCore.dll"

using Cake.Tasks.Config;

ConfigureTask<DotNetCoreConfig>(cfg =>
{
    cfg.Test.ProjectFile = @".\TestLibrary.Tests\TestLibrary.Tests.csproj";
    cfg.Publish.ProjectFile = @".\TestLibrary\TestLibrary.csproj";
});

RunTarget(Argument("target", "Default"));
