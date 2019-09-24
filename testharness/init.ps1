dotnet build ..\CakeTasks.sln
if (-not (Test-Path '.\tools\Modules\'))
{
    New-Item -ItemType Directory -Path .\tools\Modules\
}
Copy-Item -Path ..\src\Cake.Tasks\bin\Debug\netstandard2.0\*.dll -Destination .\tools\Modules\

Copy-Item -Path ..\src\Cake.Tasks.Core\bin\Debug\netstandard2.0\Cake.Tasks.Core.dll -Destination .\tools\
Copy-Item -Path ..\src\Cake.Tasks.DotNetCore\bin\Debug\netstandard2.0\Cake.Tasks.DotNetCore.dll -Destination .\tools\
