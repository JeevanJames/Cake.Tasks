#addin nuget:https://myget.org/f/cake-tasks/?package=Cake.Tasks.Core&version=1.0.0-build.11&prerelease
#addin nuget:https://myget.org/f/cake-tasks/?package=Cake.Tasks.DotNetCore&version=1.0.0-build.11&prerelease

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

Setup(ctx =>
{
   // Executed BEFORE the first task.
   Information("Running tasks...");
});

// Teardown(ctx =>
// {
//    // Executed AFTER the last task.
//    Information("Finished running tasks.");
// });

// Task("Default")
// .IsDependentOn("Build")
// .Does(() => {
//    Information("Hello Cake!");
// });

RunTarget(target);
