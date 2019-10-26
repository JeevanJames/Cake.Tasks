# Cake Tasks
Cake Tasks is a framework built on top of the [Cake build automation system](https://cakebuild.net/) that allows you to package Cake tasks into reusable NuGet packages.

If you have multiple projects that have similar build steps or work in an organization that mandates uniform build practices, you can use Cake Tasks to package up all your Cake tasks into one or more NuGet packages and share them between projects.

A typical `build.cake` file using the Cake.Tasks framework.
```cs
// Use the Cake Tasks module. This is needed.
#module nuget:?package=Cake.Tasks.Module

// Use your organization's custom packaged tasks.
// You can specify one or more such addins.
#addin nuget:?package=Cake.Tasks.MyOrg

// Add your configurations here, if needed.
ConfigureTask<MyConfig>(cfg =>
{
    // Specify configurations here
});

RunTarget(Argument("target", "Default"));
```
