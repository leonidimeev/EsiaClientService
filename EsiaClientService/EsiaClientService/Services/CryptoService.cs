using EsiaClientService.Infrastructure;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace EsiaClientService.Services;

public class CryptoService : ICryptoService
{
    private readonly CryptoServiceOptions _cryptoServiceOptions;
    private readonly IHttpClientFactory _httpClientFactory;

#pragma warning disable IDE0290 // Использовать основной конструктор
    public CryptoService(
#pragma warning restore IDE0290 // Использовать основной конструктор
        IOptions<CryptoServiceOptions> cryptoServiceOptions,
        IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        _cryptoServiceOptions = cryptoServiceOptions.Value;
    }

    public async Task<string> SignBase64Async(string thumbprint, string message, bool detached, CancellationToken cancellationToken = default)
    {
        using var httpClient = _httpClientFactory.CreateClient();

        var uri = new Uri(_cryptoServiceOptions.Url + "/api/cryptopro/sign");
        var contentForSign = new
        {
            Thumbprint = thumbprint,
            Message = message
        };
        var contentForSignSerialized = JsonConvert.SerializeObject(contentForSign);

        httpClient.DefaultRequestHeaders.Add("Detached", detached ? "True" : "False");

        var jsonContent = new StringContent(contentForSignSerialized,
            Encoding.UTF8,
            "application/json"
        );
        var response = await httpClient.PostAsync(uri, jsonContent, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    public async Task<HttpResponseMessage> SendEsiaRequestAsync(string url, string thumbprint, CancellationToken cancellationToken = default)
    {
        using var httpClient = _httpClientFactory.CreateClient();

        var requestUri = new Uri($"{_cryptoServiceOptions.Url}/api/cryptopro/sendTls12Request");
        var request = new HttpRequestMessage(HttpMethod.Post, requestUri);

        request.Headers.Add("Url", url);
        request.Headers.Add("Thumbprint", thumbprint);
        request.Headers.Add("Method", "Post");
        request.Content = new StringContent("", Encoding.UTF8, "application/x-www-form-urlencoded");

        var response = await httpClient.SendAsync(request, cancellationToken);
        return response;
    }

    public async Task<HttpResponseMessage> GetPersonDataAsync(string url, string thumbprint, string accessToken, CancellationToken cancellationToken = default)
    {
        using var httpClient = _httpClientFactory.CreateClient();

        var requestUri = new Uri($"{_cryptoServiceOptions.Url}/api/cryptopro/sendTls12Request");
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

        request.Headers.Add("Url", url);
        request.Headers.Add("Thumbprint", thumbprint);
        request.Headers.Add("Method", "Post");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = new StringContent("", Encoding.UTF8, "application/x-www-form-urlencoded");

        var response = await httpClient.SendAsync(request, cancellationToken);
        return response;
    }

    public async Task<string> GetClientSecretAsync(string message, string thumbprint, CancellationToken cancellationToken = default)
    {
        using var httpClient = _httpClientFactory.CreateClient();

        var uri = new Uri(_cryptoServiceOptions.Url + "/api/cryptopro/esiaGetClientSecret");
        var contentForSign = new StringContent(message);
        httpClient.DefaultRequestHeaders.Add("Thumbprint", thumbprint);

        var response = await httpClient.PostAsync(uri, contentForSign, cancellationToken);
        response.EnsureSuccessStatusCode();

        var receivedSecret = await response.Content.ReadAsStringAsync(cancellationToken);
        return receivedSecret[1..^1];
    }
}