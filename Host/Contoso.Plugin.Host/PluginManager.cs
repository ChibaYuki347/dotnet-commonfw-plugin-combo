using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using Contoso.Plugin.Abstractions;

namespace Contoso.Plugin.Host;

public sealed class PluginManager : IAsyncDisposable
{
    private readonly List<PluginHandle> _loaded = new();

    public async Task<IReadOnlyList<PluginHandle>> LoadFromManifestAsync(string manifestPath, CancellationToken ct = default)
    {
        var text = await File.ReadAllTextAsync(manifestPath, ct).ConfigureAwait(false);
        var manifest = JsonSerializer.Deserialize<PluginManifest>(text, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        }) ?? throw new InvalidOperationException("Invalid plugins.json");

        foreach (var e in manifest.Enabled ?? Array.Empty<PluginEntry>())
        {
            ct.ThrowIfCancellationRequested();
            var basePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, e.Path ?? string.Empty));
            if (!Directory.Exists(basePath))
                throw new DirectoryNotFoundException(basePath);

            string? mainDll = e.Main;
            if (string.IsNullOrWhiteSpace(mainDll))
            {
                var dlls = Directory.GetFiles(basePath, "*.dll", SearchOption.TopDirectoryOnly);
                mainDll = dlls.FirstOrDefault(d => Path.GetFileNameWithoutExtension(d).Equals(Path.GetFileName(basePath), StringComparison.OrdinalIgnoreCase))
                    ?? dlls.FirstOrDefault(d => Path.GetFileNameWithoutExtension(d).Equals(e.Id, StringComparison.OrdinalIgnoreCase))
                    ?? dlls.FirstOrDefault();
            }
            if (mainDll is null) throw new FileNotFoundException($"No .dll found under {basePath}");

            var alc = new PluginLoadContext(mainDll);
            var asm = alc.LoadFromAssemblyPath(mainDll);
            var pluginType = asm.GetTypes().FirstOrDefault(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
                             ?? throw new InvalidOperationException($"No IPlugin implementation found in {mainDll}");
            var instance = (IPlugin)Activator.CreateInstance(pluginType)!;
            _loaded.Add(new PluginHandle(e.Id ?? pluginType.FullName ?? "plugin", instance, alc));
        }
        return _loaded;
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var h in _loaded) h.LoadContext.Unload();
        _loaded.Clear();
        for (int i = 0; i < 5; i++) { GC.Collect(); GC.WaitForPendingFinalizers(); await Task.Delay(10); }
    }

    public sealed record PluginHandle(string Id, IPlugin Instance, AssemblyLoadContext LoadContext);
    public sealed class PluginManifest
    {
        public string[]? Probes { get; set; }
        public PluginEntry[]? Enabled { get; set; }
    }
    public sealed class PluginEntry
    {
        public string? Id { get; set; }
        public string? Path { get; set; }
        public string? Main { get; set; }
    }
}
