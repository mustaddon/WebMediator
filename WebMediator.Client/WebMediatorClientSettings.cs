namespace WebMediator.Client;

public class WebMediatorClientSettings
{
    public TypeDeserializer? TypeDeserializer { get; set; }

    public HttpMessageHandler? HttpHandler { get; set; }

    public EventStreamOptions EventStreamOptions { get; set; } = new();

    public Dictionary<string, IEnumerable<string>>? HttpHeaders { get; set; }

    public JsonSerializerOptions JsonSerializerOptions { get; set; } = new()
    {
        PropertyNameCaseInsensitive = true,
    };


    public WebMediatorClientSettings RegisterTypes(IEnumerable<Type> types)
    {
        TypeDeserializer ??= new();
        TypeDeserializer.Register(types);
        return this;
    }

    public WebMediatorClientSettings RegisterTypeAlias(string name, Type type) => RegisterTypeAlias([(name, type)]);
    public WebMediatorClientSettings RegisterTypeAlias(IEnumerable<(string Name, Type Type)> typeAlias)
    {
        TypeDeserializer ??= new();
        TypeDeserializer.Register(typeAlias);
        return this;
    }

    public WebMediatorClientSettings RegisterTypesAssignableTo<T>(params Assembly[] assemblies)
    {
        var type = typeof(T);
        return RegisterTypes(assemblies
            .Distinct()
            .SelectMany(x => x.GetTypes())
            .Where(type.IsAssignableFrom));
    }
}
