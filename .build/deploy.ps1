# Cake Tasks module
dotnet pack ./src/Cake.Tasks.Module/Cake.Tasks.Module.csproj --include-symbols --include-source -c Release /p:Version=$env:APPVEYOR_BUILD_VERSION
dotnet nuget push ./src/Cake.Tasks.Module/bin/Release/Cake.Tasks.Module.$env:APPVEYOR_BUILD_VERSION.nupkg -s $env:MYGET_FEED -k $env:MYGET_APIKEY -ss $env:MYGET_SYMBOLS_FEED -sk $env:MYGET_SYMBOLS_APIKEY

# Cake Tasks core
dotnet pack ./src/Cake.Tasks.Core/Cake.Tasks.Core.csproj --include-symbols --include-source -c Release /p:Version=$env:APPVEYOR_BUILD_VERSION
dotnet nuget push ./src/Cake.Tasks.Core/bin/Release/Cake.Tasks.Core.$env:APPVEYOR_BUILD_VERSION.nupkg -s $env:MYGET_FEED -k $env:MYGET_APIKEY -ss $env:MYGET_SYMBOLS_FEED -sk $env:MYGET_SYMBOLS_APIKEY

# Cake Tasks GitVersion plugin
dotnet pack ./plugin/Cake.Tasks.GitVersion/Cake.Tasks.GitVersion.csproj --include-symbols --include-source -c Release /p:Version=$env:APPVEYOR_BUILD_VERSION
dotnet nuget push ./plugin/Cake.Tasks.GitVersion/bin/Release/Cake.Tasks.GitVersion.$env:APPVEYOR_BUILD_VERSION.nupkg -s $env:MYGET_FEED -k $env:MYGET_APIKEY -ss $env:MYGET_SYMBOLS_FEED -sk $env:MYGET_SYMBOLS_APIKEY

# Cake Tasks .NET Core plugin
dotnet pack ./plugin/Cake.Tasks.DotNetCore/Cake.Tasks.DotNetCore.csproj --include-symbols --include-source -c Release /p:Version=$env:APPVEYOR_BUILD_VERSION
dotnet nuget push ./plugin/Cake.Tasks.DotNetCore/bin/Release/Cake.Tasks.DotNetCore.$env:APPVEYOR_BUILD_VERSION.nupkg -s $env:MYGET_FEED -k $env:MYGET_APIKEY -ss $env:MYGET_SYMBOLS_FEED -sk $env:MYGET_SYMBOLS_APIKEY

# Cake Tasks Local plugin
dotnet pack ./plugin/Cake.Tasks.Local/Cake.Tasks.Local.csproj --include-symbols --include-source -c Release /p:Version=$env:APPVEYOR_BUILD_VERSION
dotnet nuget push ./plugin/Cake.Tasks.Local/bin/Release/Cake.Tasks.Local.$env:APPVEYOR_BUILD_VERSION.nupkg -s $env:MYGET_FEED -k $env:MYGET_APIKEY -ss $env:MYGET_SYMBOLS_FEED -sk $env:MYGET_SYMBOLS_APIKEY

# Meta package
((Get-Content -path ./plugin/CakeTasks.nuspec -Raw) -replace '0.1.0',$env:APPVEYOR_BUILD_VERSION) | Set-Content -Path ./plugin/CakeTasks.nuspec
nuget pack ./plugin/CakeTasks.nuspec -OutputDirectory ./plugin -OutputFileNamesWithoutVersion
dotnet nuget push ./plugin/CakeTasks.nupkg -s $env:MYGET_FEED -k $env:MYGET_APIKEY
