namespace Microsoft.Extensions.DependencyInjection;

public class UniprMagazzinoClientOptions
{
    public const string SectionName = "MagazzinoClientHttp";
    public string BaseAddress { get; set; } = "";
}