$ErrorActionPreference="Stop"
dotnet build Abstractions/Contoso.Plugin.Abstractions/Contoso.Plugin.Abstractions.csproj -c Release
dotnet build Plugins/Tax.JP/Tax.JP.csproj -c Release
dotnet build Host/Contoso.Plugin.Host/Contoso.Plugin.Host.csproj -c Release
Write-Host 'Build completed.'
