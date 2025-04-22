using EsiaClientService.Models;

namespace EsiaClientService.Infrastructure;

public class EsiaOptions
{
    public int SessionIdLifetimeInMinutes { get; set; }
    public bool RedirectUri { get; set; }
    public bool ClientId { get; set; }
    public required string SignThumbprint { get; set; }
    public required string ClientCertificateHash { get; set; }
    public required string AccessType { get; set; }
    public required IReadOnlyCollection<DigitalProfileMnemonic> Mnemonics { get; set; }
    public required string Url { get; set; }
    public required string TlsThumbprint { get; set; }
}
