// #addin nuget:https://myget.org/f/cake-tasks/?package=Cake.Tasks.Core&version=1.0.0-build.29&prerelease
// #addin nuget:https://myget.org/f/cake-tasks/?package=Cake.Tasks.DotNetCore&version=1.0.0-build.29&prerelease
// #addin nuget:https://myget.org/f/cake-tasks/?package=Cake.Tasks.Octopus&version=1.0.0-build.29&prerelease
// #addin nuget:https://myget.org/f/cake-tasks/?package=Cake.Tasks.Sonar&version=1.0.0-build.29&prerelease

#r ".\tools\Cake.Tasks.Core.dll"
#r ".\tools\Cake.Tasks.DotNetCore.dll"
#r ".\tools\Cake.Tasks.Octopus.dll"
#r ".\tools\Cake.Tasks.Sonar.dll"

Configuration(tc =>
{
    var dncc = tc.Load<DotNetCoreConfig>();
    dncc.BuildProjectFiles = new[] { @".\TestLibrary\TestLibrary.csproj", @".\TestLibrary.Tests\TestLibrary.Tests.csproj" };
    dncc.TestProjectFiles = @".\TestLibrary.Tests\TestLibrary.Tests.csproj";
    
    var sonar = tc.Load<SonarConfig>();
    sonar.Url = "https://localhost:9001";
});

RunTarget(Argument("target", "Default"));
