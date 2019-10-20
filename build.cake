#module "nuget:https://myget.org/f/cake-tasks/?package=Cake.Tasks.Module&prerelease"

#tool "nuget:?package=GitVersion.CommandLine&version=5.0.1"

// #addin "nuget:https://myget.org/f/cake-tasks/?package=Cake.Tasks.JeevanJames&prerelease&loaddependencies=true"
#addin "nuget:https://myget.org/f/cake-tasks/?package=Cake.Tasks.Ci.AppVeyor&prerelease"
#addin "nuget:https://myget.org/f/cake-tasks/?package=Cake.Tasks.Core&prerelease"
#addin "nuget:https://myget.org/f/cake-tasks/?package=Cake.Tasks.GitVersion&prerelease"
#addin "nuget:https://myget.org/f/cake-tasks/?package=Cake.Tasks.DotNetCore&prerelease"
#addin "nuget:https://myget.org/f/cake-tasks/?package=Cake.Tasks.JeevanJames&prerelease"

ConfigureTask<DotNetCoreConfig>(cfg =>
{
    cfg.Test.Skip = true;
});

public static void DeployPackage(this ICakeContext ctx, string root, string projectName, EnvConfig env)
{
    string outputDirectory = System.IO.Path.Combine(env.Directories.BinaryOutput, projectName);
    string packagePath = System.IO.Path.Combine(outputDirectory, $"{projectName}.{env.Version.Build}.nupkg");

    string nugetFeed = ctx.EnvironmentVariable("MYGET_FEED");
    string nugetApiKey = ctx.EnvironmentVariable("MYGET_APIKEY");

    ctx.DotNetCorePack($"./{root}/{projectName}/{projectName}.csproj", new DotNetCorePackSettings
    {
        IncludeSource = true,
        IncludeSymbols = true,
        Configuration = env.Configuration,
        OutputDirectory = outputDirectory,
        ArgumentCustomization = arg => arg.Append($"/p:Version={env.Version.Build}"),
    });

    ctx.Log.Information($"Pushing package {packagePath} to {nugetFeed}");
    ctx.DotNetCoreNuGetPush(packagePath, new DotNetCoreNuGetPushSettings
    {
        ApiKey = nugetApiKey,
        Source = nugetFeed,
    });
}

Task("DeployIndividualPackages")
    .IsDependentOn("_Config")
    .Does<TaskConfig>((ctx, cfg) =>
{
    EnvConfig env = cfg.Load<EnvConfig>();
    if (!env.IsCi)
    {
        Warning("Not in CI system. Cannot deploy NuGet packages.");
        return;
    }

    ctx.DeployPackage("src", "Cake.Tasks.Module", env);
    ctx.DeployPackage("src", "Cake.Tasks.Core", env);
    ctx.DeployPackage("plugin", "Cake.Tasks.Ci.AppVeyor", env);
    ctx.DeployPackage("plugin", "Cake.Tasks.Ci.Tfs", env);
    ctx.DeployPackage("plugin", "Cake.Tasks.DotNetCore", env);
    ctx.DeployPackage("plugin", "Cake.Tasks.GitVersion", env);
    ctx.DeployPackage("plugin", "Cake.Tasks.JeevanJames", env);
});

Task("DeployRecipePackage")
    .IsDependentOn("DeployIndividualPackages")
    .Does<TaskConfig>((ctx, cfg) =>
{
    EnvConfig env = cfg.Load<EnvConfig>();
    if (!env.IsCi)
        return;

    string nuspecFile = System.IO.Path.Combine(env.Directories.Working, "metapackage", "CakeTasks.nuspec");
    string nuspecContent = System.IO.File.ReadAllText(nuspecFile)
        .Replace("0.1.0", env.Version.Build);
    System.IO.File.WriteAllText(nuspecFile, nuspecContent);

    string nugetFeed = EnvironmentVariable("MYGET_FEED");
    string nugetApiKey = EnvironmentVariable("MYGET_APIKEY");

    string outputDirectory = System.IO.Path.Combine(env.Directories.BinaryOutput, "metapackage");
    string packagePath = System.IO.Path.Combine(outputDirectory, $"Cake.Tasks.JeevanJames.{env.Version.Build}.nupkg");

    NuGetPack(nuspecFile, new NuGetPackSettings
    {
        OutputDirectory = outputDirectory,
        Version = env.Version.Build,
    });

    ctx.DotNetCoreNuGetPush(packagePath, new DotNetCoreNuGetPushSettings
    {
        ApiKey = nugetApiKey,
        Source = nugetFeed,
    });
});

RunTarget(Argument("target", "Default"));
