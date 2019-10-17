#module "nuget:https://myget.org/f/cake-tasks/?package=Cake.Tasks.Module&prerelease"

#tool "nuget:?package=GitVersion.CommandLine"

#addin "nuget:https://myget.org/f/cake-tasks/?package=Cake.Tasks.JeevanJames&prerelease"
#addin "nuget:https://myget.org/f/cake-tasks/?package=Cake.Tasks.Ci.AppVeyor&prerelease"

ConfigureTask<DotNetCoreConfig>(cfg =>
{
    cfg.Test.Skip = true;
});

RunTarget(Argument("target", "Default"));
