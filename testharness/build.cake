// #addin nuget:https://myget.org/f/cake-tasks/?package=Cake.Tasks.Core&version=1.0.0-build.22&prerelease
// #addin nuget:https://myget.org/f/cake-tasks/?package=Cake.Tasks.DotNetCore&version=1.0.0-build.22&prerelease

#r ".\tools\Cake.Tasks.Core.dll"
#r ".\tools\Cake.Tasks.DotNetCore.dll"

using Cake.Tasks.Core;
using Cake.Tasks.DotNetCore;

Configuration(tc =>
{
    var dncc = tc.Load<DotNetCoreConfig>();
    dncc.BuildProjectFiles = new[] { @".\TestLibrary\TestLibrary.csproj", @".\TestLibrary.Tests\TestLibrary.Tests.csproj" };
    dncc.TestProjectFiles = @".\TestLibrary.Tests\TestLibrary.Tests.csproj";
});

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

RunTarget(target);
