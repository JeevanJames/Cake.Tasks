#module "nuget:https://myget.org/f/cake-tasks/?package=Cake.Tasks.Module&prerelease"

#addin "nuget:https://myget.org/f/cake-tasks/?package=Cake.Tasks.JeevanJames&prerelease&loaddependencies=true"
#addin "nuget:https://myget.org/f/cake-tasks/?package=Cake.Tasks.Ci.AppVeyor&prerelease"

using System.Collections;

ConfigureTask<DotNetCoreConfig>(cfg =>
{
    cfg.Test.Skip = true;
});

Task("ListEnvs")
    .IsDependeeOf("Build")
    .Does(() =>
{
    foreach (DictionaryEntry entry in Environment.GetEnvironmentVariables())
    {
        Information($"{entry.Key} = {entry.Value}");
    }
});

RunTarget(Argument("target", "Default"));
