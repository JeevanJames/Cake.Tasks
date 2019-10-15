[CmdletBinding()]
Param(
	[string]$Version
)

if (-Not $Version) {
	Write-Host -ForegroundColor Red "Please specify the version to publish"
	Write-Host -ForegroundColor Cyan -NoNewLine "USAGE: "
	Write-Host "deploy-nuget.ps1 -version <version>"
	exit -1
}

dotnet pack ./src/Cake.Tasks.Module/Cake.Tasks.Module.csproj --include-symbols --include-source -c Release /p:Version=$Version
dotnet nuget push ./src/Cake.Tasks.Module/bin/Release/Cake.Tasks.Module.$Version.nupkg -s https://api.nuget.org/v3/index.json

dotnet pack ./src/Cake.Tasks.Core/Cake.Tasks.Core.csproj --include-symbols --include-source -c Release /p:Version=$Version
dotnet nuget push ./src/Cake.Tasks.Core/bin/Release/Cake.Tasks.Core.$Version.nupkg -s https://api.nuget.org/v3/index.json

dotnet pack ./plugin/Cake.Tasks.GitVersion/Cake.Tasks.GitVersion.csproj --include-symbols --include-source -c Release /p:Version=$Version
dotnet nuget push ./plugin/Cake.Tasks.GitVersion/bin/Release/Cake.Tasks.GitVersion.$Version.nupkg -s https://api.nuget.org/v3/index.json
