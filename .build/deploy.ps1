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

PublishPackage "src" "Cake.Tasks.Module"
PublishPackage "src" "Cake.Tasks.Core"
PublishPackage "plugin" "Cake.Tasks.GitVersion"
PublishPackage "plugin" "Cake.Tasks.Ci.Tfs"
PublishPackage "plugin" "Cake.Tasks.Ci.AppVeyor"
PublishPackage "plugin" "Cake.Tasks.DotNetCore"

# Meta package
WriteBanner "Publishing Cake Tasks addins metapackage"
New-Item -ItemType Directory -Path ./metapackage/ref/netstandard2.0
New-Item -ItemType Directory -Path ./metapackage/lib/netstandard2.0

Copy-Item -Path ./src/Cake.Tasks.Core/bin/Release/netstandard2.0/Cake.Tasks.Core.dll -Destination .\metapackage\ref\netstandard2.0
Copy-Item -Path ./plugin/Cake.Tasks.DotNetCore/bin/Release/netstandard2.0/Cake.Tasks.DotNetCore.dll -Destination .\metapackage\ref\netstandard2.0
Copy-Item -Path ./plugin/Cake.Tasks.GitVersion/bin/Release/netstandard2.0/Cake.Tasks.GitVersion.dll -Destination .\metapackage\ref\netstandard2.0

Copy-Item -Path ./src/Cake.Tasks.Core/bin/Release/netstandard2.0/Cake.Tasks.Core.dll -Destination .\metapackage\lib\netstandard2.0
Copy-Item -Path ./plugin/Cake.Tasks.DotNetCore/bin/Release/netstandard2.0/Cake.Tasks.DotNetCore.dll -Destination .\metapackage\lib\netstandard2.0
Copy-Item -Path ./plugin/Cake.Tasks.GitVersion/bin/Release/netstandard2.0/Cake.Tasks.GitVersion.dll -Destination .\metapackage\lib\netstandard2.0

((Get-Content -path ./metapackage/CakeTasks.nuspec -Raw) -replace '0.1.0',$env:APPVEYOR_BUILD_VERSION) | Set-Content -Path ./metapackage/CakeTasks.nuspec
nuget pack ./metapackage/CakeTasks.nuspec -OutputDirectory ./metapackage -OutputFileNamesWithoutVersion
dotnet nuget push ./metapackage/Cake.Tasks.JeevanJames.nupkg -s $env:MYGET_FEED -k $env:MYGET_APIKEY
