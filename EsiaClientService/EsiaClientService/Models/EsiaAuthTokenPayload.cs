using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;

namespace EsiaClientService.Models;

public class EsiaAuthTokenPayload
{
    /// <summary>
    /// Токен доступа
    /// </summary>
    [JsonProperty("access_token")]
    public required string AccessToken { get; set; }

    public EsiaAccessTokenPayload GetAccessToken()
    {
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(AccessToken);

        return new EsiaAccessTokenPayload
        {
            Nbf = token.Claims.FirstOrDefault(c => c.Type == "nbf")?.Value!,
            Permissions = token.Claims.FirstOrDefault(c => c.Type == "permissions")?.Value!,
            Scope = token.Claims.FirstOrDefault(c => c.Type == "scope")?.Value!,
            Iss = token.Claims.FirstOrDefault(c => c.Type == "iss")?.Value!,
            Sid = token.Claims.FirstOrDefault(c => c.Type == "urn:esia:sid")?.Value!,
            SbjId = Convert.ToInt32(token.Claims.FirstOrDefault(c => c.Type == "urn:esia:sbj_id")?.Value),
            Exp = Convert.ToInt64(token.Claims.FirstOrDefault(c => c.Type == "exp")?.Value),
            Iat = Convert.ToInt64(token.Claims.FirstOrDefault(c => c.Type == "iat")?.Value),
            ClientId = Convert.ToInt32(token.Claims.FirstOrDefault(c => c.Type == "client_id")?.Value),
            PermissionsUrl = token.Claims.FirstOrDefault(c => c.Type == "permissions_url")?.Value!
        };
    }

    /// <summary>
    /// Токен обновления
    /// </summary>
    [JsonProperty("refresh_token")]
    public required string RefreshToken { get; set; }

    /// <summary>
    /// Токен
    /// </summary>
    [JsonProperty("id_token")]
    public required string IdToken { get; set; }

    public EsiaIdToken GetIdToken()
    {
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(IdToken);

        return new EsiaIdToken
        {
            Aud = token.Claims.FirstOrDefault(c => c.Type == "aud")?.Value!,
            Sub = token.Claims.FirstOrDefault(c => c.Type == "sub")?.Value!,
            Nbf = Convert.ToInt64(token.Claims.FirstOrDefault(c => c.Type == "nbf")?.Value),
            Amr = token.Claims.FirstOrDefault(c => c.Type == "amr")?.Value!,
            UrnEsiaAmd = token.Claims.FirstOrDefault(c => c.Type == "urn:esia:amd")?.Value!,
            AuthTime = Convert.ToInt64(token.Claims.FirstOrDefault(c => c.Type == "auth_time")?.Value),
            Iss = token.Claims.FirstOrDefault(c => c.Type == "iss")?.Value!,
            UrnEsiaSid = token.Claims.FirstOrDefault(c => c.Type == "urn:esia:sid")?.Value!,
            UrnEsiaSbj = JsonConvert.DeserializeObject<UrnEsiaSbj>(token.Claims.FirstOrDefault(c => c.Type == "urn:esia:sbj")?.Value!)!,
            Exp = Convert.ToInt64(token.Claims.FirstOrDefault(c => c.Type == "exp")?.Value),
            Iat = Convert.ToInt64(token.Claims.FirstOrDefault(c => c.Type == "iat")?.Value)
        };
    }

    /// <summary>
    /// Идентификатор сессии
    /// </summary>
    [JsonProperty("state")]
    public required string State { get; set; }

    /// <summary>
    /// Тип токена
    /// </summary>
    [JsonProperty("token_type")]
    public required string TokenType { get; set; }

    /// <summary>
    /// Время жизни токена
    /// </summary>
    [JsonProperty("expires_in")]
    public required string ExpiresIn { get; set; }

    private string[] Parts => AccessToken?.Split('.') ?? [];

    /// <summary>
    /// Сообщение для проверки подписи
    /// </summary>
    public string GetMessage()
    {
        if (string.IsNullOrEmpty(AccessToken))
        {
            return null!;
        }

        if (Parts.Length < 2)
        {
            throw new InvalidOperationException($"При расшифровке токена доступа произошла ошибка. Токен: {AccessToken}");
        }

        return Parts[0] + "." + Parts[1];
    }

    /// <summary>
    /// Сигнатура подписи
    /// </summary>
    public string? GetSignature()
    {
        if (string.IsNullOrEmpty(AccessToken))
        {
            return null;
        }

        if (Parts.Length < 2)
        {
            throw new InvalidOperationException($"При расшифровке токена доступа произошла ошибка. Токен: {AccessToken}");
        }

        return Parts[2];
    }
}