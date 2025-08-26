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

        var absVersion = typeof(IPlugin).Assembly.GetName().Version ?? new Version(0, 0, 0, 0);
        var absSemVer = new Version(absVersion.Major, absVersion.Minor, absVersion.Build < 0 ? 0 : absVersion.Build);

        foreach (var e in manifest.Enabled ?? Array.Empty<PluginEntry>())
        {
            ct.ThrowIfCancellationRequested();
            var basePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, e.Path ?? string.Empty));
            if (!Directory.Exists(basePath))
                throw new DirectoryNotFoundException(basePath);

            // 互換性チェック（min/maxAbstractions が指定されている場合）
            if (TryParseVersion(e.MinAbstractions, out var minV) && absSemVer < minV)
                throw new InvalidOperationException($"{e.Id} requires Abstractions >= {minV}, but host has {absSemVer}");
            if (TryParseVersion(e.MaxAbstractions, out var maxV) && absSemVer >= maxV)
                throw new InvalidOperationException($"{e.Id} requires Abstractions < {maxV}, but host has {absSemVer}");

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
        public string? MinAbstractions { get; set; }
        public string? MaxAbstractions { get; set; }
    }

    private static bool TryParseVersion(string? s, out Version v)
    {
        if (!string.IsNullOrWhiteSpace(s) && Version.TryParse(s, out v!)) return true;
        v = new Version(0,0,0);
        return false;
    }
}
