#module "nuget:https://myget.org/f/cake-tasks/?package=Cake.Tasks.Module&prerelease"

#addin "nuget:https://myget.org/f/cake-tasks/?package=Cake.Tasks.JeevanJames&prerelease&loaddependencies=true"
#addin "nuget:https://myget.org/f/cake-tasks/?package=Cake.Tasks.Ci.AppVeyor&prerelease"

ConfigureTask<DotNetCoreConfig>(cfg =>
{
    cfg.Test.Skip = true;
});

RunTarget(Argument("target", "Default"));
