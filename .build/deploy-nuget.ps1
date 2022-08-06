[CmdletBinding()]
Param(
	[string]$Version
)

if (-Not $Version) {
	Write-Host -ForegroundColor Red "Please specify the version to publish"
	Write-Host -ForegroundColor Cyan -NoNewLine "USAGE: "
	Write-Host "deploy-nuget.ps1 -version <version>"
    nuget list Cake.Tasks.Core
	exit -1
}

# Cake Tasks module
dotnet pack ./src/Cake.Tasks.Module/Cake.Tasks.Module.csproj --include-symbols --include-source -c Release /p:Version=$Version
dotnet nuget push ./src/Cake.Tasks.Module/bin/Release/Cake.Tasks.Module.$Version.nupkg -s https://api.nuget.org/v3/index.json

# Cake Tasks core
dotnet pack ./src/Cake.Tasks.Core/Cake.Tasks.Core.csproj --include-symbols --include-source -c Release /p:Version=$Version
dotnet nuget push ./src/Cake.Tasks.Core/bin/Release/Cake.Tasks.Core.$Version.nupkg -s https://api.nuget.org/v3/index.json

# Cake Tasks Git plugin
dotnet pack ./plugin/Cake.Tasks.Git/Cake.Tasks.Git.csproj --include-symbols --include-source -c Release /p:Version=$Version
dotnet nuget push ./plugin/Cake.Tasks.Git/bin/Release/Cake.Tasks.Git.$Version.nupkg -s https://api.nuget.org/v3/index.json

# Cake Tasks GitVersion plugin
dotnet pack ./plugin/Cake.Tasks.GitVersion/Cake.Tasks.GitVersion.csproj --include-symbols --include-source -c Release /p:Version=$Version
dotnet nuget push ./plugin/Cake.Tasks.GitVersion/bin/Release/Cake.Tasks.GitVersion.$Version.nupkg -s https://api.nuget.org/v3/index.json

# Cake Tasks TFS CI plugin
dotnet pack ./plugin/Cake.Tasks.Ci.Tfs/Cake.Tasks.Ci.Tfs.csproj --include-symbols --include-source -c Release /p:Version=$Version
dotnet nuget push ./plugin/Cake.Tasks.Ci.Tfs/bin/Release/Cake.Tasks.Ci.Tfs.$Version.nupkg -s https://api.nuget.org/v3/index.json

# Cake Tasks AppVeyor CI plugin
dotnet pack ./plugin/Cake.Tasks.Ci.AppVeyor/Cake.Tasks.Ci.AppVeyor.csproj --include-symbols --include-source -c Release /p:Version=$Version
dotnet nuget push ./plugin/Cake.Tasks.Ci.AppVeyor/bin/Release/Cake.Tasks.Ci.AppVeyor.$Version.nupkg -s https://api.nuget.org/v3/index.json

# Cake Tasks .NET Core plugin
dotnet pack ./plugin/Cake.Tasks.DotNetCore/Cake.Tasks.DotNetCore.csproj --include-symbols --include-source -c Release /p:Version=$Version
dotnet nuget push ./plugin/Cake.Tasks.DotNetCore/bin/Release/Cake.Tasks.DotNetCore.$Version.nupkg -s https://api.nuget.org/v3/index.json
