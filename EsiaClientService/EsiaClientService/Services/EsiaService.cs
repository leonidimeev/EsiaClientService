using EsiaClientService.Infrastructure;
using EsiaClientService.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace EsiaClientService.Services;

public class EsiaService : IEsiaService
{
    private readonly ILogger<EsiaService> _logger;
    private readonly IMemoryCache _memoryCache;
    private readonly ICryptoService _cryptoService;
    private readonly EsiaOptions _esiaOptions;

#pragma warning disable IDE0290 // Использовать основной конструктор
    public EsiaService(
#pragma warning restore IDE0290 // Использовать основной конструктор
        IOptions<EsiaOptions> esiaOptions,
        ICryptoService cryptoService,
        IMemoryCache memoryCache,
        ILogger<EsiaService> logger)
    {
        _esiaOptions = esiaOptions.Value;
        _cryptoService = cryptoService;
        _memoryCache = memoryCache;
        _logger = logger;
    }

    /// <summary>
    /// Создаёт URL для аутентификации в ЕСИА.
    /// </summary>
    /// <param name="externalSessionId">Идентификатор сессии.</param>
    /// <param name="redirectUri">URI для редиректа после аутентификации.</param>
    /// <param name="mnemonicName">Название мнемоника для запроса прав.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>URL для аутентификации в ЕСИА.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<string> AuthCreateAsync(
        string externalSessionId,
        string redirectUri,
        string mnemonicName, 
        CancellationToken cancellationToken
        )
    {
        _logger.LogInformation("Создание запроса аутентификации для сессии {SessionId}", externalSessionId);

        // Проверка входных параметров
        if (string.IsNullOrEmpty(externalSessionId))
            throw new ArgumentNullException(nameof(externalSessionId));
        if (string.IsNullOrEmpty(redirectUri))
            throw new ArgumentNullException(nameof(redirectUri));
        if (string.IsNullOrEmpty(mnemonicName))
            throw new ArgumentNullException(nameof(mnemonicName));

        var state = Guid.NewGuid();
        var stateString = state.ToString("D");
        var expiration = TimeSpan.FromMinutes(_esiaOptions.SessionIdLifetimeInMinutes);

        var redirectUriDecoded = redirectUri
            .Replace("%3A", ":")
            .Replace("%2F", "/");

        var sessionData = new EsiaSessionData
        {
            State = stateString,
            RedirectUri = redirectUriDecoded,
            MnemonicName = mnemonicName,
            CachedData = []
        };

        _memoryCache.Set(externalSessionId, sessionData, expiration);
        _memoryCache.Set(stateString, sessionData, expiration);

        // Создаем ClientSecret
        var redirectUriInternal = _esiaOptions.RedirectUri;
        var msgBuilder = new StringBuilder();
        var timestamp = DateTime.UtcNow.ToString("yyyy.MM.dd HH:mm:ss +0000");

        msgBuilder
            .Append(_esiaOptions.ClientId)
            .Append("openid")
            .Append(timestamp)
            .Append(state)
            .Append(redirectUriInternal);

        var clientSecretUrlSafe = await GenerateClientSecretAsync(msgBuilder.ToString(), cancellationToken);

        // Формируем Uri
        var queryParams = new StringBuilder();
        queryParams.Append($"?client_secret={clientSecretUrlSafe}");
        queryParams.Append($"&client_id={_esiaOptions.ClientId}");
        queryParams.Append($"&timestamp={timestamp}");
        queryParams.Append($"&state={state}");
        queryParams.Append($"&redirect_uri={redirectUriInternal}");
        queryParams.Append($"&client_certificate_hash={_esiaOptions.ClientCertificateHash}");
        queryParams.Append($"&response_type=code");
        queryParams.Append($"&access_type={_esiaOptions.AccessType}");
        queryParams.Append($"&scope=openid");

        var mnemonic = _esiaOptions.Mnemonics.FirstOrDefault(x => x.Name.Equals(mnemonicName)) 
            ?? throw new ArgumentException($"Мнемоник '{mnemonicName}' не найден в конфигурации.");
        if (mnemonic.Scopes.Count == 0)
            throw new InvalidOperationException($"Для мнемоника '{mnemonicName}' не заданы scopes.");

        var permissions = new[]
        {
            new
            {
                expire = 262800,
                scopes = mnemonic.Scopes.Select(s => new { sysname = s }),
                purposes = new[] { new { sysname = mnemonicName } },
                sysname = mnemonicName,
                actions = new[]
                {
                    new { sysname = "ALL_ACTIONS_TO_DATA" }
                }
            }
        };

        var permissionsJson = JsonConvert.SerializeObject(permissions);
        queryParams.Append($"&permissions={Base64UrlEncoder.Encode(permissionsJson)}");

        // Получение url-safe записи и запрос в ЕСИА
        var esiaRequest = _esiaOptions.Url + "/aas/oauth2/v2/ac" + queryParams.ToString()
            .Replace("+", "%2B")
            .Replace(":", "%3A")
            .Replace(" ", "+");

        _logger.LogDebug("Сгенерирован state: {State}", stateString);

        return esiaRequest;
    }

    public async Task<EsiaRedirectResponse> GetInfoRedirectAsync(
        string state, 
        string code, 
        string error,
        string error_description, 
        CancellationToken cancellationToken
        )
    {
        _logger.LogInformation("Обработка редиректа от ЕСИА с state={State}", state);

        if (error != null)
        {
            throw new InvalidOperationException($"{error.ToUpper()}: {error_description}");
        }

        if (!_memoryCache.TryGetValue(state, out EsiaSessionData? sessionData))
        {
            _logger.LogWarning("Сессия с state={State} не найдена в кэше.", state);
            throw new KeyNotFoundException("Сессия не найдена.");
        }

        var result = new EsiaRedirectResponse { Url = sessionData!.RedirectUri };

        if (!_memoryCache.TryGetValue(state, out string? externalSessionId))
        {
            throw new KeyNotFoundException("Session not found");
        }

        var redirectBuilder = new StringBuilder(sessionData.RedirectUri);
        redirectBuilder.Append($"?externalSessionId={sessionData.ExternalSessionId}");
        result.Url = redirectBuilder.ToString();

        // Получение токенов авторизации
        var tokenInfoResponse = await SendEsiaAuthAsync(
            state, 
            code, 
            cancellationToken)
            .ConfigureAwait(false);

        var tokenResponse = await tokenInfoResponse
            .Content
            .ReadAsStringAsync(cancellationToken)
            .ConfigureAwait(false);

        // Есть ошибочные ответы из ЕСИА, приходящие в виде html
        if (await IsHtmlAsync(tokenResponse) || !tokenInfoResponse.IsSuccessStatusCode)
        {
            _memoryCache.Set($"esia_error_of_{state}", tokenResponse,
                TimeSpan.FromMinutes(_esiaOptions.SessionIdLifetimeInMinutes));
            result.Success = false;
            result.Error = "htmlError";
            result.ErrorDescription = tokenResponse;
            return result;
        }

        var token = JsonConvert.DeserializeObject<EsiaAuthTokenPayload>(tokenResponse) ?? throw new InvalidOperationException("token == null");
        var accessToken = token.GetAccessToken();

        if (!_memoryCache.TryGetValue($"mnemonic_of_{state}", out string? mnemonic))
        {
            throw new KeyNotFoundException("Mnemonic not found");
        }

        var mnemonicUrls = _esiaOptions.Mnemonics.Where(x => x.Name.Equals(mnemonic)).First().Urls;
        var cacheKey = $"esia_data_{state}";
        var cachedData = new Dictionary<string, JObject>();

        // Запрос по каждому url в мнемонике
        var tasks = new List<Task>();
        foreach (var url in mnemonicUrls)
        {
            async Task getMnemonicTask()
            {
                JObject response;
                try
                {
                    response = await SendEsiaRequestAsync(
                        url, 
                        accessToken.SbjId.ToString(), 
                        token.AccessToken,
                        cancellationToken)
                        .ConfigureAwait(false);
                    cachedData[url] = response;
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException($"Error while trying to get mnemonic url {url}, {e.Message}");
                }
            }

            tasks.Add(getMnemonicTask());
        }
        try
        {
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
        catch (Exception)
        {
            var exceptions = tasks.Where(t => t.IsFaulted).Select(t => t.Exception);
            throw new AggregateException("Ошибка при запросе данных из ЕСИА", exceptions!);
        }

        // Сохраняем все полученные данные в MemoryCache
        _memoryCache.Set(cacheKey, cachedData,
            TimeSpan.FromMinutes(_esiaOptions.SessionIdLifetimeInMinutes));

        result.Success = true;
        return result;
    }

    public List<JObject> GetSubjectInfo(
        string externalSessionId,
        string mnemonicName,
        CancellationToken cancellationToken
        )
    {
        // Получение state по externalSessionId
        if (!_memoryCache.TryGetValue(externalSessionId, out string? state))
        {
            throw new InvalidOperationException("NOT_FOUND: Сессия не найдена");
        }

        // Получение сохраненных данных из кэша
        var cacheKey = $"esia_data_{state}";
        if (!_memoryCache.TryGetValue(cacheKey, out Dictionary<string, JObject>? cachedData))
        {
            throw new InvalidOperationException("NOT_FOUND: Данные сессии не найдены");
        }

        // Получение URL-ов для запрошенного мнемоника
        var mnemonicConfig = _esiaOptions.Mnemonics.FirstOrDefault(x => x.Name.Equals(mnemonicName)) 
            ?? throw new InvalidOperationException($"NOT_FOUND: Мнемоник {mnemonicName} не найден в конфигурации");

        // Фильтрация данных по URL-ам мнемоника
        var result = new List<JObject>();
        foreach (var url in mnemonicConfig.Urls)
        {
            if (cachedData!.TryGetValue(url, out JObject? data))
            {
                result.Add(data);
            }
            else
            {
                // Если данных для URL нет, можно добавить пустой объект или пропустить
                result.Add([]);
            }
        }

        return result;
    }

    private async Task<JObject> SendEsiaRequestAsync(
        string url, 
        string oid, 
        string accessToken,
        CancellationToken cancellationToken = default
        )
    {
        var urlReplaced = url.Replace("{oid}", oid);

        var rawUrlResponse = await _cryptoService.GetPersonDataAsync(
            _esiaOptions.Url + urlReplaced,
            _esiaOptions.TlsThumbprint, 
            accessToken, 
            cancellationToken)
            .ConfigureAwait(false);

        var rawUrlResponseContent = await rawUrlResponse
            .Content
            .ReadAsStringAsync(cancellationToken)
            .ConfigureAwait(false);

        if (!rawUrlResponse.IsSuccessStatusCode)
        {
            if (rawUrlResponse.StatusCode == HttpStatusCode.NotFound)
            {
                throw new InvalidOperationException($"NOT_FOUND");
            }

            throw new InvalidOperationException("SendEsiaRequestAsync mnemonic url not successful query, url - {url}, statusCode - {rawUrlResponse.StatusCode}, content - {rawUrlResponseContent}");
        }

        return ParseResponseAsJObject(rawUrlResponseContent, url);
    }

    private async Task<HttpResponseMessage> SendEsiaAuthAsync(
        string state,
        string code, 
        CancellationToken cancellationToken = default
        )
    {
        // Создание ClientSecret
        var msgBuilder = new StringBuilder();
        var timestamp = DateTime.UtcNow.ToString("yyyy.MM.dd HH:mm:ss +0000");
        msgBuilder
            .Append(_esiaOptions.ClientId)
            .Append("openid")
            .Append(timestamp)
            .Append(state)
            .Append(_esiaOptions.RedirectUri)
            .Append(code);

        var clientSecretUrlSafe = await GenerateClientSecretAsync(msgBuilder.ToString(), cancellationToken);

        // Сборка запроса
        var queryParams = new StringBuilder();
        queryParams.Append($"?client_secret={clientSecretUrlSafe}");
        queryParams.Append($"&client_id={_esiaOptions.ClientId}");
        queryParams.Append($"&scope=openid");
        queryParams.Append($"&timestamp={timestamp}");
        queryParams.Append($"&state={state}");
        queryParams.Append($"&redirect_uri={_esiaOptions.RedirectUri}");
        queryParams.Append($"&client_certificate_hash={_esiaOptions.ClientCertificateHash}");
        queryParams.Append($"&code={code}");

        // принимает значение «authorization_code», если авторизационный код обменивается на маркер доступа
        queryParams.Append($"&grant_type=authorization_code");
        queryParams.Append($"&token_type=Bearer");

        var esiaRequest = _esiaOptions.Url + "/aas/oauth2/v3/te" + queryParams.ToString()
            .Replace("+", "%2B")
            .Replace(":", "%3A")
            .Replace(" ", "+");

        return await _cryptoService.SendEsiaRequestAsync(
            esiaRequest,
            _esiaOptions.TlsThumbprint,
            cancellationToken)
            .ConfigureAwait(false);
    }

    private static readonly Regex HtmlRegex = new(@"<[^>]*(>|$)", RegexOptions.CultureInvariant | RegexOptions.Compiled, TimeSpan.FromSeconds(5));
    private static ValueTask<bool> IsHtmlAsync(string input)
    {
        return ValueTask.FromResult(HtmlRegex.IsMatch(input));
    }

    private static JObject ParseResponseAsJObject(
        string rawUrlResponseContent, 
        string url
        )
    {
        JObject? response;

        // Some of the mnemonic url responses come wrapped in array
        var isArray = rawUrlResponseContent[0] == '[';
        if (isArray)
        {
            var responseItems = JsonConvert.DeserializeObject<List<JObject>>(rawUrlResponseContent) ?? throw new ArgumentException(
                    $"Mnemonic url response not found, url - {url}, content - {rawUrlResponseContent}");
            if (responseItems.Count > 1)
            {
                throw new ArgumentException(
                    $"Mnemonic url more than 1 response found, url - {url}, content - {rawUrlResponseContent}");
            }

            response = responseItems.FirstOrDefault();
        }
        else
        {
            response = JsonConvert.DeserializeObject<JObject>(rawUrlResponseContent);
        }

        if (response == null)
        {
            throw new ArgumentException(
                $"Mnemonic url response not found, url - {url}, content - {rawUrlResponseContent}");
        }

        return response;
    }

    private async Task<string> GenerateClientSecretAsync(string message, CancellationToken cancellationToken)
    {
        var msgBase64 = Base64UrlEncoder.Encode(message);
        var clientSecret = await _cryptoService.GetClientSecretAsync(msgBase64, _esiaOptions.SignThumbprint, cancellationToken);
        return clientSecret.Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}
