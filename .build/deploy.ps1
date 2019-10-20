function WriteBanner() {
    param($message)
    $Separator = "=" * $message.Length
    Write-Host $Separator
    Write-Host $message -ForegroundColor Magenta
    Write-Host $Separator
}

function PublishPackage($root, $name) {
    WriteBanner "Publishing $name"
    dotnet pack ./$root/$name/$name.csproj --include-symbols --include-source -c Release /p:Version=$env:APPVEYOR_BUILD_VERSION
    dotnet nuget push ./$root/$name/bin/Release/$name.$env:APPVEYOR_BUILD_VERSION.nupkg -s $env:MYGET_FEED -k $env:MYGET_APIKEY -ss $env:MYGET_SYMBOLS_FEED -sk $env:MYGET_SYMBOLS_APIKEY
}

# PublishPackage "src" "Cake.Tasks.Module"
# PublishPackage "src" "Cake.Tasks.Core"
# PublishPackage "plugin" "Cake.Tasks.GitVersion"
# PublishPackage "plugin" "Cake.Tasks.Ci.Tfs"
# PublishPackage "plugin" "Cake.Tasks.Ci.AppVeyor"
# PublishPackage "plugin" "Cake.Tasks.DotNetCore"

# Meta package
WriteBanner "Publishing Cake Tasks addins metapackage"
((Get-Content -path ./metapackage/CakeTasks.nuspec -Raw) -replace '0.1.0',$env:APPVEYOR_BUILD_VERSION) | Set-Content -Path ./metapackage/CakeTasks.nuspec
nuget pack ./metapackage/CakeTasks.nuspec -OutputDirectory ./metapackage -OutputFileNamesWithoutVersion
dotnet nuget push ./metapackage/Cake.Tasks.JeevanJames.nupkg -s $env:MYGET_FEED -k $env:MYGET_APIKEY
