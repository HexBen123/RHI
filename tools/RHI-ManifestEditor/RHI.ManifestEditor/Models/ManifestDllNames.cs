using System.Text.Json.Serialization;

namespace RHI.ManifestEditor.Models;

public class ManifestDllNames
{
    [JsonPropertyName("reshade")]
    public string? ReShade { get; set; }

    [JsonPropertyName("dc")]
    public string? Dc { get; set; }
}
