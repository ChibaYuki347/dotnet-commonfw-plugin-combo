#!/usr/bin/env bash
set -euo pipefail
RUNTIME="${1:-win-x64}"
dotnet publish Plugins/Tax.JP/Tax.JP.csproj -c Release -r "$RUNTIME" --self-contained false -o dist/Plugins/Tax.JP/v1.0.0
dotnet publish Host/Contoso.Plugin.Host/Contoso.Plugin.Host.csproj -c Release -r "$RUNTIME" --self-contained false -o dist/Host
cp -f Host/Contoso.Plugin.Host/plugins.json dist/Host
echo "Publish completed. Run dist/Host/Contoso.Plugin.Host.exe"
