#addin nuget:https://myget.org/f/cake-tasks/?package=Cake.Tasks.Core&version=1.0.0-build.22&prerelease
#addin nuget:https://myget.org/f/cake-tasks/?package=Cake.Tasks.DotNetCore&version=1.0.0-build.22&prerelease

// #r "..\src\Cake.Tasks.Core\bin\Debug\netstandard2.0\Cake.Tasks.Core.dll"
// #r "..\src\Cake.Tasks.DotNetCore\bin\Debug\netstandard2.0\Cake.Tasks.DotNetCore.dll"

using Cake.Tasks.DotNetCore;

Configuration(tc =>
{
    tc.Set(Config.BuildProjectFiles, @".\TestHarness.sln");
});

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

RunTarget(target);
