using System.Text.Json;
using Contoso.Plugin.Abstractions;
using Contoso.Plugin.Host;

Console.WriteLine("Host starting...");
var manager = new PluginManager();
var plugins = await manager.LoadFromManifestAsync(Path.Combine(AppContext.BaseDirectory, "plugins.json"));

// 日本顧客（JP）とその他（US）のサンプル入力
var jpReq = new TaxRequest("JP", 1000m);
var usReq = new TaxRequest("US", 1000m);

foreach (var p in plugins)
{
    var jpRes = await p.Instance.ExecuteAsync(JsonSerializer.Serialize(jpReq));
    var usRes = await p.Instance.ExecuteAsync(JsonSerializer.Serialize(usReq));
    Console.WriteLine($"[{p.Id}] JP => {jpRes}");
    Console.WriteLine($"[{p.Id}] US => {usRes}");
}

await manager.DisposeAsync();
Console.WriteLine("Host done.");
