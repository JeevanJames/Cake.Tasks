// #addin nuget:https://myget.org/f/cake-tasks/?package=Cake.Tasks.Core&version=1.0.0-build.29&prerelease
// #addin nuget:https://myget.org/f/cake-tasks/?package=Cake.Tasks.DotNetCore&version=1.0.0-build.29&prerelease
// #addin nuget:https://myget.org/f/cake-tasks/?package=Cake.Tasks.Octopus&version=1.0.0-build.29&prerelease
// #addin nuget:https://myget.org/f/cake-tasks/?package=Cake.Tasks.Sonar&version=1.0.0-build.29&prerelease

// #tool nuget:?package=MSBuild.SonarQube.Runner.Tool&version=4.6.0
// #addin nuget:?package=Cake.Sonar&version=1.1.22

#r ".\tools\Cake.Tasks.Core.dll"
#r ".\tools\Cake.Tasks.DotNetCore.dll"
#r ".\tools\Cake.Tasks.Octopus.dll"
// #r ".\tools\Cake.Tasks.Sonar.dll"

using Cake.Common.Tools.DotNetCore.Test;

ConfigureTask<DotNetCoreConfig>(cfg =>
{
    cfg.Build.ProjectFiles = new[] { @".\TestLibrary\TestLibrary.csproj", @".\TestLibrary.Tests\TestLibrary.Tests.csproj" };
    cfg.Test.ProjectFiles = @".\TestLibrary.Tests\TestLibrary.Tests.csproj";
    cfg.Test.Settings = new DotNetCoreTestSettings
    {
        NoBuild = true
    };
});

RunTarget(Argument("target", "Default"));
