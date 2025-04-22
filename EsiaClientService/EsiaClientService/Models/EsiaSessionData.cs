using Newtonsoft.Json.Linq;

namespace EsiaClientService.Models;

public class EsiaSessionData
{
    public string? ExternalSessionId { get; set; }
    public string? State { get; internal set; }
    public string? RedirectUri { get; set; }
    public string? MnemonicName { get; set; }
    public Dictionary<string, JObject>? CachedData { get; set; }
}
