# Cake Tasks module
Write-Host "============================"
Write-Host "Publishing Cake Tasks module" -ForegroundColor Magenta
Write-Host "============================"
dotnet pack ./src/Cake.Tasks.Module/Cake.Tasks.Module.csproj --include-symbols --include-source -c Release /p:Version=$env:APPVEYOR_BUILD_VERSION
dotnet nuget push ./src/Cake.Tasks.Module/bin/Release/Cake.Tasks.Module.$env:APPVEYOR_BUILD_VERSION.nupkg -s $env:MYGET_FEED -k $env:MYGET_APIKEY -ss $env:MYGET_SYMBOLS_FEED -sk $env:MYGET_SYMBOLS_APIKEY

# Cake Tasks core
Write-Host "=================================="
Write-Host "Publishing Cake Tasks core library" -ForegroundColor Magenta
Write-Host "=================================="
dotnet pack ./src/Cake.Tasks.Core/Cake.Tasks.Core.csproj --include-symbols --include-source -c Release /p:Version=$env:APPVEYOR_BUILD_VERSION
dotnet nuget push ./src/Cake.Tasks.Core/bin/Release/Cake.Tasks.Core.$env:APPVEYOR_BUILD_VERSION.nupkg -s $env:MYGET_FEED -k $env:MYGET_APIKEY -ss $env:MYGET_SYMBOLS_FEED -sk $env:MYGET_SYMBOLS_APIKEY

# Cake Tasks GitVersion plugin
Write-Host "======================================"
Write-Host "Publishing Cake Tasks GitVersion addin" -ForegroundColor Magenta
Write-Host "======================================"
dotnet pack ./plugin/Cake.Tasks.GitVersion/Cake.Tasks.GitVersion.csproj --include-symbols --include-source -c Release /p:Version=$env:APPVEYOR_BUILD_VERSION
dotnet nuget push ./plugin/Cake.Tasks.GitVersion/bin/Release/Cake.Tasks.GitVersion.$env:APPVEYOR_BUILD_VERSION.nupkg -s $env:MYGET_FEED -k $env:MYGET_APIKEY -ss $env:MYGET_SYMBOLS_FEED -sk $env:MYGET_SYMBOLS_APIKEY

# Cake Tasks .NET Core plugin
Write-Host "====================================="
Write-Host "Publishing Cake Tasks .NET Core addin" -ForegroundColor Magenta
Write-Host "====================================="
dotnet pack ./plugin/Cake.Tasks.DotNetCore/Cake.Tasks.DotNetCore.csproj --include-symbols --include-source -c Release /p:Version=$env:APPVEYOR_BUILD_VERSION
dotnet nuget push ./plugin/Cake.Tasks.DotNetCore/bin/Release/Cake.Tasks.DotNetCore.$env:APPVEYOR_BUILD_VERSION.nupkg -s $env:MYGET_FEED -k $env:MYGET_APIKEY -ss $env:MYGET_SYMBOLS_FEED -sk $env:MYGET_SYMBOLS_APIKEY

# Cake Tasks Local plugin
Write-Host "================================="
Write-Host "Publishing Cake Tasks local addin" -ForegroundColor Magenta
Write-Host "================================="
dotnet pack ./plugin/Cake.Tasks.Local/Cake.Tasks.Local.csproj --include-symbols --include-source -c Release /p:Version=$env:APPVEYOR_BUILD_VERSION
dotnet nuget push ./plugin/Cake.Tasks.Local/bin/Release/Cake.Tasks.Local.$env:APPVEYOR_BUILD_VERSION.nupkg -s $env:MYGET_FEED -k $env:MYGET_APIKEY -ss $env:MYGET_SYMBOLS_FEED -sk $env:MYGET_SYMBOLS_APIKEY

# Meta package
Write-Host "========================================"
Write-Host "Publishing Cake Tasks addins metapackage" -ForegroundColor Magenta
Write-Host "========================================"
New-Item -ItemType Directory -Path ./metapackage/ref/netstandard2.0
New-Item -ItemType Directory -Path ./metapackage/lib/netstandard2.0

Copy-Item -Path ./src/Cake.Tasks.Core/bin/Release/netstandard2.0/Cake.Tasks.Core.dll -Destination .\metapackage\ref\netstandard2.0
Copy-Item -Path ./plugin/Cake.Tasks.DotNetCore/bin/Release/netstandard2.0/Cake.Tasks.DotNetCore.dll -Destination .\metapackage\ref\netstandard2.0
Copy-Item -Path ./plugin/Cake.Tasks.GitVersion/bin/Release/netstandard2.0/Cake.Tasks.GitVersion.dll -Destination .\metapackage\ref\netstandard2.0
Copy-Item -Path ./plugin/Cake.Tasks.Local/bin/Release/netstandard2.0/Cake.Tasks.Local.dll -Destination .\metapackage\ref\netstandard2.0

Copy-Item -Path ./src/Cake.Tasks.Core/bin/Release/netstandard2.0/Cake.Tasks.Core.dll -Destination .\metapackage\lib\netstandard2.0
Copy-Item -Path ./plugin/Cake.Tasks.DotNetCore/bin/Release/netstandard2.0/Cake.Tasks.DotNetCore.dll -Destination .\metapackage\lib\netstandard2.0
Copy-Item -Path ./plugin/Cake.Tasks.GitVersion/bin/Release/netstandard2.0/Cake.Tasks.GitVersion.dll -Destination .\metapackage\lib\netstandard2.0
Copy-Item -Path ./plugin/Cake.Tasks.Local/bin/Release/netstandard2.0/Cake.Tasks.Local.dll -Destination .\metapackage\lib\netstandard2.0

((Get-Content -path ./metapackage/CakeTasks.nuspec -Raw) -replace '0.1.0',$env:APPVEYOR_BUILD_VERSION) | Set-Content -Path ./metapackage/CakeTasks.nuspec
nuget pack ./metapackage/CakeTasks.nuspec -OutputDirectory ./metapackage -OutputFileNamesWithoutVersion
dotnet nuget push ./metapackage/Cake.Tasks.JeevanJames.nupkg -s $env:MYGET_FEED -k $env:MYGET_APIKEY
