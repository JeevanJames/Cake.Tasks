#module "nuget:https://myget.org/f/cake-tasks/?package=Cake.Tasks.Module&prerelease"

#addin "nuget:https://myget.org/f/cake-tasks/?package=Cake.Tasks.JeevanJames&prerelease&loaddependencies=true"
#addin "nuget:https://myget.org/f/cake-tasks/?package=Cake.Tasks.Ci.AppVeyor&prerelease"

ConfigureTask<DotNetCoreConfig>(cfg =>
{
    cfg.Test.Skip = true;
});

public static void DeployPackage(this ICakeContext ctx, string root, string projectName, TaskConfig cfg)
{
    var env = cfg.Load<EnvConfig>();

    ctx.DotNetCorePack($"./{root}/{projectName}/{projectName}.csproj", new DotNetCorePackSettings
    {
        IncludeSource = true,
        IncludeSymbols = true,
        Configuration = env.Configuration,
        OutputDirectory = System.IO.Path.Combine(env.Directories.BinaryOutput, projectName),
        ArgumentCustomization = arg => arg.Append($"/p:Version={env.Version.Build}"),
    });
}

Task("DeployPackages")
    .IsDependentOn("CI")
    .Does<TaskConfig>((ctx, cfg) =>
{
    ctx.DeployPackage("src", "Cake.Tasks.Module", cfg);
    ctx.DeployPackage("src", "Cake.Tasks.Core", cfg);
    ctx.DeployPackage("plugin", "Cake.Tasks.Ci.AppVeyor", cfg);
    ctx.DeployPackage("plugin", "Cake.Tasks.Ci.Tfs", cfg);
    ctx.DeployPackage("plugin", "Cake.Tasks.DotNetCore", cfg);
    ctx.DeployPackage("plugin", "Cake.Tasks.GitVersion", cfg);
});

RunTarget(Argument("target", "Default"));
