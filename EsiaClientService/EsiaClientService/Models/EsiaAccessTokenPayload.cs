using Newtonsoft.Json;

namespace EsiaClientService.Models;

public class EsiaAccessTokenPayload
{
    /// <summary>
    /// Время начала действия
    /// в секундах с 1 января 1970 г. 00:00:00 GMT,
    /// т.е. маркер нельзя обрабатывать до наступления указанного времени
    /// </summary>
    [JsonProperty("nbf")]
    public required string Nbf { get; set; }
    
    /// <summary>
    /// Список скопов на которые выдано разрешение
    /// </summary>
    [JsonProperty("permissions")]
    public required string Permissions { get; set; }

    /// <summary>
    /// Области доступа
    /// </summary>
    [JsonProperty("scope")]
    public required string Scope { get; set; }

    /// <summary>
    /// Организация, выпустившая маркер
    /// указывается URL ЕСИА
    /// </summary>
    [JsonProperty("iss")]
    public required string Iss { get; set; }

    /// <summary>
    /// Идентификатор маркера
    /// набор случайных символов,
    /// имеющий вид 128-битного идентификатора, сгенерированного
    /// по стандарту UUID
    /// </summary>
    [JsonProperty("urn:esia:sid")]
    public required string Sid { get; set; }

    /// <summary>
    /// Идентификатор субъекта в Цифровом профиле
    /// </summary>
    [JsonProperty("urn:esia:sbj_id")]
    public int SbjId { get; set; }

    /// <summary>
    /// Время прекращения действия
    /// в секундах с 1 января 1970 г. 00:00:00 GMT
    /// </summary>
    [JsonProperty("exp")]
    public long Exp { get; set; }

    /// <summary>
    /// Время выдачи
    /// в секундах с 1 января 1970 г. 00:00:00 GMT
    /// </summary>
    [JsonProperty("iat")]
    public long Iat { get; set; }

    /// <summary>
    /// Адресат маркера (мнемоника системы)
    /// </summary>
    [JsonProperty("client_id")]
    public int ClientId { get; set; }
    
    /// <summary>
    /// URL разрешений
    /// </summary>
    [JsonProperty("permissions_url")]
    public required string PermissionsUrl { get; set; }
}