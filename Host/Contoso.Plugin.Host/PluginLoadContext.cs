using System.Reflection;
using System.Runtime.Loader;

namespace Contoso.Plugin.Host;

sealed class PluginLoadContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _resolver;
    private static readonly string ContractAsmName = "Contoso.Plugin.Abstractions";

    public PluginLoadContext(string pluginMainAssemblyPath, bool isCollectible = true)
        : base(nameof(PluginLoadContext) + ":" + Path.GetFileNameWithoutExtension(pluginMainAssemblyPath), isCollectible)
        => _resolver = new AssemblyDependencyResolver(pluginMainAssemblyPath);

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        if (assemblyName.Name?.Equals(ContractAsmName, StringComparison.OrdinalIgnoreCase) == true)
            return null; // Let Default ALC resolve the shared contract to keep type identity.
        var path = _resolver.ResolveAssemblyToPath(assemblyName);
        return path is null ? null : LoadFromAssemblyPath(path);
    }

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        => _resolver.ResolveUnmanagedDllToPath(unmanagedDllName) is string p ? LoadUnmanagedDllFromPath(p) : IntPtr.Zero;
}
