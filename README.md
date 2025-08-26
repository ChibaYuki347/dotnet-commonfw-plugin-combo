# .NET 8 Plugin Template (ALC + AssemblyDependencyResolver)
- Abstractions: Contoso.Plugin.Abstractions (IPlugin)
- Host: dynamic loader using ALC + ADR
- Sample plugin: Plugins/Tax.JP

## Quick start
1) Build: `./build.ps1` or `dotnet build` on each project
2) Publish: `./publish.ps1` (or `./publish.sh win-x64`)
3) Run: `dist/Host/Contoso.Plugin.Host.exe`

The host will load the plugin from `plugins.json` and execute it. The custom ALC delegates the contract assembly to the Default context and resolves others via ADR based on the plugin's `*.deps.json`.
