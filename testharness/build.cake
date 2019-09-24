// #addin nuget:https://myget.org/f/cake-tasks/?package=Cake.Tasks.Core&version=1.0.0-build.22&prerelease
// #addin nuget:https://myget.org/f/cake-tasks/?package=Cake.Tasks.DotNetCore&version=1.0.0-build.22&prerelease

#r ".\tools\Cake.Tasks.Core.dll"
#r ".\tools\Cake.Tasks.DotNetCore.dll"

using Cake.Tasks.DotNetCore;

Configuration(tc =>
{
    Information(tc.Resolve<object>(Config.BuildProjectFiles) ?? "<No Config Value>");
    tc.Set(Config.BuildProjectFiles, @".\TestLibrary\TestLibrary.csproj");
});

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

RunTarget(target);
