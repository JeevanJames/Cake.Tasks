#tool "nuget:?package=GitVersion.CommandLine"

#r ".\tools\Cake.Tasks.Core.dll"
#r ".\tools\Cake.Tasks.GitVersion.dll"
#r ".\tools\Cake.Tasks.DotNetCore.dll"

ConfigureTask<DotNetCoreConfig>(cfg =>
{
    cfg.Test.ProjectFile = @".\TestLibrary.Tests\TestLibrary.Tests.csproj";
    
    var profiles = new List<PublishProfile>();
    profiles.Add(new NuGetPackagePublishProfile("TestLibrary", @"./TestLibrary/TestLibrary.csproj"));
    cfg.Publish.Profiles = profiles;
});

RunTarget(Argument("target", "Default"));
