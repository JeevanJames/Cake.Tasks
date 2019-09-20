dotnet build ..\CakeTasks.sln
if (-not (Test-Path '.\tools\Modules\'))
{
    New-Item -ItemType Directory -Path .\tools\Modules\
}
Copy-Item -Path ..\src\Cake.Tasks\bin\Debug\netstandard2.0\Cake.Tasks.Module.dll -Destination .\tools\Modules\
