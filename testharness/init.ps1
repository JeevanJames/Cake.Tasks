dotnet build ..\CakeTasks.sln
if (-not (Test-Path '.\tools\Modules\'))
{
    New-Item -ItemType Directory -Path .\tools\Modules\
}
Copy-Item -Path ..\src\Cake.Tasks\bin\Debug\netstandard2.0\*.dll -Destination .\tools\Modules\

Copy-Item -Path ..\src\Cake.Tasks.Core\bin\Debug\netstandard2.0\Cake.Tasks.Core.dll -Destination .\tools\
Copy-Item -Path ..\src\Cake.Tasks.DotNetCore\bin\Debug\netstandard2.0\Cake.Tasks.DotNetCore.dll -Destination .\tools\
Copy-Item -Path ..\src\Cake.Tasks.Local\bin\Debug\netstandard2.0\Cake.Tasks.Local.dll -Destination .\tools\
Copy-Item -Path ..\src\Cake.Tasks.Octopus\bin\Debug\netstandard2.0\Cake.Tasks.Octopus.dll -Destination .\tools\
# Copy-Item -Path ..\src\Cake.Tasks.Sonar\bin\Debug\netstandard2.0\Cake.Tasks.Sonar.dll -Destination .\tools\
