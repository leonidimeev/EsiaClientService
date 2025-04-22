using Newtonsoft.Json;

namespace EsiaClientService.Models;

public class EsiaIdToken
{
    /// <summary>
    /// адресат маркера
    /// указывается client_id системы, направившей запрос
    ///  на аутентификацию
    /// </summary>
    [JsonProperty("aud")]
    public required string Aud { get; set; }

    /// <summary>
    /// идентификатор субъекта («sub»), в качестве значения указывается OID. Этот
    /// идентификатор уникален для каждого субъекта, зарегистрированного в ЕСИА,
    /// и остается неизменным при последующих аутентификациях; адресат маркера
    ///    («aud»), указывается client_id системы, направившей запрос
    /// на аутентификацию;
    /// </summary>
    [JsonProperty("sub")]
    public required string Sub { get; set; }

    /// <summary>
    /// время начала действия («nbf») – в секундах с 1 января 1970 г. 00:00:00 GMT,
    /// т.е. маркер нельзя обрабатывать до наступления указанного времени;
    /// </summary>
    [JsonProperty("nbf")]
    public long Nbf { get; set; }

    /// <summary>
    /// метод аутентификации («amr», приватное обозначение), может принимать два
    /// значения: «DS» (электронная подпись) или «PWD» (пароль).
    /// </summary>
    [JsonProperty("amr")]
    public required string Amr { get; set; }

    /// <summary>
    /// способ авторизации («urn:esia:amd»), может принимать два значения: «DS»
    /// (электронная подпись) или «PWD2 (пароль);время выдачи («iat»), указывается
    ///     в секундах с 1 января 1970 г. 00:00:00 GMT;
    /// </summary>
    [JsonProperty("urn:esia:amd")]
    public required string UrnEsiaAmd { get; set; }

    /// <summary>
    /// время аутентификации («auth_time») – время, когда произошла
    /// аутентификация пользователя, указывается в секундах с 1 января 1970 г.
    /// 00:00:00 GMT;
    /// </summary>
    [JsonProperty("auth_time")]
    public long AuthTime { get; set; }

    /// <summary>
    /// организация, выпустившая маркер («iss»), указывается URL ЕСИА
    /// </summary>
    [JsonProperty("iss")]
    public required string Iss { get; set; }

    /// <summary>
    /// внутренний идентификатор сессии ЕСИА («urn:esia:sid»
    /// </summary>
    [JsonProperty("urn:esia:sid")]
    public required string UrnEsiaSid { get; set; }

    /// <summary>
    /// начало блока описания субъекта вызова сессии («urn:esia:sbj»)
    /// </summary>
    [JsonProperty("urn:esia:sbj")]
    public required UrnEsiaSbj UrnEsiaSbj { get; set; }

    /// <summary>
    /// время прекращения действия («exp»), указывается в секундах
    /// с 1 января 1970 г. 00:00:00 GMT;
    /// </summary>
    [JsonProperty("exp")]
    public long Exp { get; set; }

    /// <summary>
    /// Время выдачи
    /// в секундах с 1 января 1970 г. 00:00:00 GMT
    /// </summary>
    [JsonProperty("iat")]
    public long Iat { get; set; }
}

public class UrnEsiaSbj
{
    /// <summary>
    /// статус учетной записи («urn:esia:sbj:lvl»), может принимать одно из трех
    /// значений: «KD» – УЗ ребенка, «PR» – подтвержденная УЗ, «P» – УЗ с одним
    ///     из статусов: «упрощенная», «стандартная» и «упрощенная, готовая
    ///    к подтверждению»;
    /// </summary>
    [JsonProperty("urn:esia:sbj:lvl")]
    public required string UrnEsiaSbjLvl { get; set; }

    /// <summary>
    /// тип субъекта («urn:esia:sbj:typ»), может принимать различные значения,
    /// например: «P» – физическое лицо;
    /// </summary>
    [JsonProperty("urn:esia:sbj:typ")]
    public required string UrnEsiaSbjTyp { get; set; }

    /// <summary>
    /// признак подтвержденности субъекта («urn:esia:sbj:is_tru») – «is trusted» – УЗ
    /// пользователя подтверждена. Параметр отсутствует, если УЗ не подтверждена;
    /// </summary>
    [JsonProperty("urn:esia:sbj:is_tru")]
    public bool UrnEsiaSbjIsTru { get; set; }

    /// <summary>
    /// OID субъекта («urn:esia:sbj:oid») – OID УЗ пользователя;
    /// </summary>
    [JsonProperty("urn:esia:sbj:oid")]
    public required string UrnEsiaSbjOid { get; set; }

    /// <summary>
    /// псевдоним субъекта («urn:esia:sbj:nam») – внутренний для ЕСИА псевдоним
    /// пользователя;
    /// </summary>
    [JsonProperty("urn:esia:sbj:nam")]
    public required string UrnEsiaSbjNam { get; set; }
}