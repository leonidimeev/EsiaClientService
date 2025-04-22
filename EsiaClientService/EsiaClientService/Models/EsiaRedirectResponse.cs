namespace EsiaClientService.Models;

public class EsiaRedirectResponse
{
    public string? Error { get; set; }
    public string? ErrorDescription { get; set; }
    public string? Url { get; set; }
    public bool Success { get; set; }
}
