// #tool nuget:?package=MSBuild.SonarQube.Runner.Tool&version=4.6.0
#tool "nuget:?package=OctopusTools&version=6.13.1"

// #addin nuget:https://myget.org/f/cake-tasks/?package=Cake.Tasks.Core&version=1.0.0-build.29&prerelease
// #addin nuget:https://myget.org/f/cake-tasks/?package=Cake.Tasks.DotNetCore&version=1.0.0-build.29&prerelease
// #addin nuget:https://myget.org/f/cake-tasks/?package=Cake.Tasks.Octopus&version=1.0.0-build.29&prerelease
// #addin nuget:https://myget.org/f/cake-tasks/?package=Cake.Tasks.Sonar&version=1.0.0-build.29&prerelease

// #addin nuget:?package=Cake.Sonar&version=1.1.22

#r ".\tools\Cake.Tasks.Core.dll"
#r ".\tools\Cake.Tasks.DotNetCore.dll"
#r ".\tools\Cake.Tasks.Octopus.dll"
// #r ".\tools\Cake.Tasks.Sonar.dll"

using Cake.Common.Tools.DotNetCore.Test;

ConfigureTask<DotNetCoreConfig>(cfg =>
{
    cfg.Test.ProjectFiles = @".\TestLibrary.Tests\TestLibrary.Tests.csproj";
    cfg.Test.Settings = new DotNetCoreTestSettings
    {
        NoBuild = true
    };
    cfg.Publish.ProjectFile = @".\TestLibrary\TestLibrary.csproj";
});

ConfigureTask<OctopusConfig>((octo, cfg) =>
{
    octo.PackageId = "MyPackage";
    octo.Pack.BasePath = @".\TestLibrary\bin\Release\netstandard2.0\publish";
    octo.Pack.Version = (Func<string>)(() =>
    {
        var ci = cfg.Load<CiConfig>();
        return $"1.0.0-build.{ci.BuildNumber}";
    });
});

RunTarget(Argument("target", "Default"));
