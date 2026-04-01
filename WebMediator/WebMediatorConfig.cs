namespace WebMediator;

public sealed class WebMediatorConfig
{
    public bool CreatingInstancesOnEmptyRequests { get; set; } = true;

    public Func<HttpContext, Task<IResult>>? TypeNotFoundResult { get; set; }

    public TypeDeserializer? TypeDeserializer { get; set; }

    public JsonSerializerOptions JsonSerialization { get; set; } = new()
    {
        PropertyNameCaseInsensitive = true,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false,
    };

    public WebMediatorConfig DisableCreatingInstancesOnEmptyRequests(bool disable = true)
    {
        CreatingInstancesOnEmptyRequests = !disable;
        return this;
    }

    public WebMediatorConfig RegisterTypes(IEnumerable<Type> types)
    {
        TypeDeserializer ??= new();
        TypeDeserializer.Register(types);
        return this;
    }

    public WebMediatorConfig RegisterTypeAlias(string name, Type type) => RegisterTypeAlias([(name, type)]);
    public WebMediatorConfig RegisterTypeAlias(IEnumerable<(string Name, Type Type)> typeAlias)
    {
        TypeDeserializer ??= new();
        TypeDeserializer.Register(typeAlias);
        return this;
    }

    public WebMediatorConfig RegisterTypesAssignableTo<T>(params Assembly[] assemblies)
    {
        var type = typeof(T);
        return RegisterTypes(assemblies
            .Distinct()
            .SelectMany(x => x.GetTypes())
            .Where(type.IsAssignableFrom));
    }

    public WebMediatorConfig RegisterNotAbstractTypesAssignableTo<T>(params Assembly[] assemblies)
    {
        var type = typeof(T);
        return RegisterTypes(assemblies
            .Distinct()
            .SelectMany(x => x.GetTypes())
            .Where(x => type.IsAbstract && type.IsAssignableFrom(x)));
    }

    public WebMediatorConfig RegisterNotAbstractTypesFromAssembly(Assembly assembly)
    {
        return RegisterTypes(assembly.GetTypes()
            .Where(x => !x.IsAbstract && !x.IsAttribute() && !x.IsException()));
    }
}