using EsiaClientService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace EsiaClientService.Controllers;

[ApiController]
[Route("[controller]")]
public class EsiaSessionController(
    IEsiaService esiaService) : ControllerBase
{
    private readonly IEsiaService _esiaService = esiaService;

    /// <summary>
    /// Создание сессии и получение ссылки для авторизации субъекта ЦПГ
    /// </summary>
    /// <param name="externalId"></param>
    /// <param name="redirectUri"></param>
    /// <param name="mnemonic"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost(Name = "CreateSession")]
    public async Task<IActionResult> CreateSessionAsync(
        [FromQuery] string externalId, 
        [FromQuery] string redirectUri,
        [FromQuery] string mnemonic,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(externalId))
            return BadRequest("externalSessionId is required");

        if (string.IsNullOrEmpty(redirectUri))
            return BadRequest("redirectUri is required");

        if (!Uri.IsWellFormedUriString(redirectUri, UriKind.Absolute))
            return BadRequest("redirectUri must be a valid absolute URL");

        var generatedLink = await _esiaService.AuthCreateAsync(
            externalId, 
            redirectUri, 
            mnemonic, 
            cancellationToken)
            .ConfigureAwait(false);

        return Redirect(generatedLink);
    }

    /// <summary>
    /// Редирект метод для получения ответа из ЕСИА
    /// </summary>
    /// <param name="state"></param>
    /// <param name="code"></param>
    /// <param name="error"></param>
    /// <param name="error_description"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("Redirect")]
    public async Task<IActionResult> RedirectAsync(
        [FromQuery] string state,
        [FromQuery] string code,
        [FromQuery] string error, 
        [FromQuery] string error_description,
        CancellationToken cancellationToken)
    {
        var result = await _esiaService.GetInfoRedirectAsync(
            state,
            code,
            error,
            error_description,
            cancellationToken)
            .ConfigureAwait(false);

        if (!result.Success)
        {
            if (result.Error == "htmlError")
            {
                return Content(result.ErrorDescription!, "text/html; charset=utf-8");
            }

            var uriBuilder = new UriBuilder(result.Url!);
            var queryParams = new Dictionary<string, string> { { "error", result.Error! } };
            uriBuilder.Query = QueryHelpers.AddQueryString(uriBuilder.Query, queryParams!);
            return Redirect(uriBuilder.ToString());
        }

        return Redirect(result.Url!);
    }

    /// <summary>
    /// Получение информации о субъекте
    /// </summary>
    /// <param name="externalSessionId"></param>
    /// <param name="mnemonic"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("GetInfo")]
    public IActionResult GetInfo(
        [FromQuery] string externalSessionId, 
        [FromQuery] string mnemonic,
        CancellationToken cancellationToken)
    {
        var result = _esiaService.GetSubjectInfo(externalSessionId, mnemonic, cancellationToken);
        return Ok(result);
    }
}
