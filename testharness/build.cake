#addin nuget:https://myget.org/f/cake-tasks/?package=Cake.Tasks.Core&version=1.0.0-build.16&prerelease
#addin nuget:https://myget.org/f/cake-tasks/?package=Cake.Tasks.DotNetCore&version=1.0.0-build.16&prerelease

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

RunTarget(target);
