using System.Text;

namespace EsiaClientService.Models;

public class DigitalProfileMnemonic
{
    /// <summary>
    /// Название мнемоники
    /// </summary>
    public required string Name { get; set; }
    
    /// <summary>
    /// Название требуемых scope
    /// </summary>
    public required List<string> Scopes { get; set; }

    /// <summary>
    /// Список запрашиваемых url
    /// </summary>
    public required List<string> Urls { get; set; }

    public string GetScopeString()
    {
        var builder = new StringBuilder();
        foreach (var item in Scopes)
        {
            builder.Append(' ').Append(item);
        }

        return builder.ToString().Trim();
    }
}