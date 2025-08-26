using System.Text.Json;
using Contoso.Plugin.Abstractions;
using Contoso.Plugin.Host;
using Contoso.Framework;

Console.WriteLine("Host starting...");
// バージョンの可視化
var abstractionsVer = typeof(IPlugin).Assembly.GetName().Version?.ToString() ?? "unknown";
var frameworkVer = typeof(DefaultTaxCalculator).Assembly.GetName().Version?.ToString() ?? "unknown";
Console.WriteLine($"Abstractions: {abstractionsVer}, Framework: {frameworkVer}");
var manager = new PluginManager();
var plugins = await manager.LoadFromManifestAsync(Path.Combine(AppContext.BaseDirectory, "plugins.json"));
foreach (var p in plugins)
{
    try
    {
        var ver = p.Instance.GetType().Assembly.GetName().Version?.ToString() ?? "unknown";
        Console.WriteLine($"Loaded plugin: {p.Id} v{ver}");
    }
    catch { /* best-effort logging */ }
}

// 日本顧客（JP）とその他（US）のサンプル入力
var jpReq = new TaxRequest("JP", 1000m);
var usReq = new TaxRequest("US", 1000m);

// 共通FW（NuGet）既定の計算器（0%）
ITaxCalculator defaultCalc = new DefaultTaxCalculator();

// 1) まずプラグインに問合せ（対象国なら上書き） 2) なければ共通FWでフォールバック
async Task<string> EvaluateAsync(TaxRequest req)
{
    foreach (var p in plugins)
    {
        var json = await p.Instance.ExecuteAsync(JsonSerializer.Serialize(req));
        // プラグインは対象外なら Applied=false で返す
        var parsed = JsonSerializer.Deserialize<TaxResult>(json);
        if (parsed?.Applied == true) return json;
    }
    // フォールバック（共通FW）
    return JsonSerializer.Serialize(defaultCalc.Calculate(req));
}

Console.WriteLine($"JP => {await EvaluateAsync(jpReq)}");
Console.WriteLine($"US => {await EvaluateAsync(usReq)}");

await manager.DisposeAsync();
Console.WriteLine("Host done.");
