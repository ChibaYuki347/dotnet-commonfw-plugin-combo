namespace Contoso.Plugin.Abstractions;
public interface IPlugin
{
    string Id { get; }
    string Version { get; }
    Task<string> ExecuteAsync(string input, CancellationToken ct = default);
}
