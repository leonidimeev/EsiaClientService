using EsiaClientService.Models;
using Newtonsoft.Json.Linq;

namespace EsiaClientService.Services;

public interface IEsiaService
{
    /// <summary>
    /// Получение авторизационного кода
    /// Реализует метод получения авторизационного кода (v2/ac) описанный в методичке
    /// https://digital.gov.ru/uploaded/files/metodicheskierekomendatsiipoispolzovaniyuesiav334.pdf
    /// </summary>
    /// <param name="externalSessionId"></param>
    /// <param name="redirectUri"></param>
    /// <param name="mnemonicName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<string> AuthCreateAsync(
        string externalSessionId,
        string redirectUri,
        string mnemonicName, 
        CancellationToken cancellationToken
        );

    /// <summary>
    /// Получение персональных данных субъекта ЦПГ
    /// </summary>
    /// <param name="state"></param>
    /// <param name="code"></param>
    /// <param name="error"></param>
    /// <param name="error_description"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<EsiaRedirectResponse> GetInfoRedirectAsync(
        string state, 
        string code, 
        string error, 
        string error_description,
        CancellationToken cancellationToken
        );

    /// <summary>
    /// Получение персональных данных субъекта ЦПГ
    /// </summary>
    /// <param name="externalSessionId"></param>
    /// <param name="mnemonic"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    List<JObject> GetSubjectInfo(
        string externalSessionId, 
        string mnemonic, 
        CancellationToken token
        );
}