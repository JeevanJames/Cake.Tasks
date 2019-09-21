#addin nuget:https://myget.org/f/cake-tasks/?package=Cake.Tasks.Core&prerelease
#addin nuget:https://myget.org/f/cake-tasks/?package=Cake.Tasks.DotNetCore&prerelease

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

Setup(ctx =>
{
   // Executed BEFORE the first task.
   Information("Running tasks...");
});

Teardown(ctx =>
{
   // Executed AFTER the last task.
   Information("Finished running tasks.");
});

Task("Default")
.Does(() => {
   Information("Hello Cake!");
});

RunTarget(target);
