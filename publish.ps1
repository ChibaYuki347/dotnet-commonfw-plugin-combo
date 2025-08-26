param([string]$Runtime="win-x64")
$ErrorActionPreference="Stop"
dotnet publish Plugins/Tax.JP/Tax.JP.csproj -c Release -r $Runtime --self-contained false -o dist/Plugins/Tax.JP/v1.0.0
dotnet publish Host/Contoso.Plugin.Host/Contoso.Plugin.Host.csproj -c Release -r $Runtime --self-contained false -o dist/Host
Copy-Item Host/Contoso.Plugin.Host/plugins.json dist/Host -Force
Write-Host 'Publish completed. Run dist/Host/Contoso.Plugin.Host.exe'
