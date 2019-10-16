# Delete tools folder
Get-ChildItem .\tools -Recurse | Remove-Item -Force
Remove-Item -LiteralPath ".\tools" -Force -Recurse

# Remove __output folder
Get-ChildItem .\__output -Recurse | Remove-Item -Force
Remove-Item -LiteralPath ".\__output" -Force -Recurse
