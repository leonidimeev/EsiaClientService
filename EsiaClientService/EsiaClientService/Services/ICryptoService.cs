namespace EsiaClientService.Services;

public interface ICryptoService
{
    /// <summary>
    /// Подписывает Base64 сообщение
    /// </summary>
    /// <param name="thumbprint">Отпечаток используемого CryptoService сертификата</param>
    /// <param name="message">Сообщение</param>
    /// <param name="detached">Флаг</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Подписанное сообщение в Base64</returns>
    Task<string> SignBase64Async(string thumbprint, string message, bool detached,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Обращение к прокси методу для получения токенов доступа
    /// </summary>
    /// <param name="thumbprint">Отпечаток используемого CryptoService сертификата</param>
    /// <param name="url">Адрес запроса</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Подписанное сообщение в Base64</returns>
    Task<HttpResponseMessage> SendEsiaRequestAsync(string url, string thumbprint,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Обращение к прокси методу для получения персональных данных субъекта ЦПГ
    /// </summary>
    /// <param name="url"></param>
    /// <param name="thumbprint"></param>
    /// <param name="accessToken"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<HttpResponseMessage> GetPersonDataAsync(string url, string thumbprint, string accessToken,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Подписывает Base64 сообщение
    /// </summary>
    /// <param name="message">Сообщение</param>
    /// <param name="thumbprint">Отпечаток используемого CryptoService сертификата</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Подписанное сообщение в Base64</returns>
    Task<string> GetClientSecretAsync(string message, string thumbprint,
        CancellationToken cancellationToken = default);
}