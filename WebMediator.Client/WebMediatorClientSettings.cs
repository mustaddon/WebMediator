namespace WebMediator.Client;

public class WebMediatorClientSettings
{
    public TypeDeserializer? TypeDeserializer { get; set; }

    public HttpMessageHandler? HttpHandler { get; set; }

    public Dictionary<string, IEnumerable<string>>? HttpHeaders { get; set; }

    public JsonSerializerOptions JsonSerializerOptions { get; set; } = new()
    {
        PropertyNameCaseInsensitive = true,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false,
    };
}
