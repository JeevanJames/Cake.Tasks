#tool "nuget:?package=GitVersion.CommandLine"

#r ".\tools\Cake.Tasks.Core.dll"
#r ".\tools\Cake.Tasks.GitVersion.dll"
#r ".\tools\Cake.Tasks.DotNetCore.dll"

ConfigureTask<EnvConfig>(cfg =>
{
    // cfg.Publishers.AddAspNetCore("TestLibrary", @"./TestLibrary/TestLibrary.csproj");
});

RunTarget(Argument("target", "Default"));
