#addin nuget:https://myget.org/f/cake-tasks/?package=Cake.Tasks.Core&prerelease
#addin nuget:https://myget.org/f/cake-tasks/?package=Cake.Tasks.DotNetCore&prerelease

// #r "..\src\Cake.Tasks.Core\bin\Debug\netstandard2.0\Cake.Tasks.Core.dll"
// #r "..\src\Cake.Tasks.DotNetCore\bin\Debug\netstandard2.0\Cake.Tasks.DotNetCore.dll"

using Cake.Tasks.DotNetCore;

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

RunTarget(target);
