using Microsoft.Extensions.Configuration;
using Magazzino.ClientHttp;
using Magazzino.ClientHttp.Abstraction;

namespace Microsoft.Extensions.DependencyInjection;

public static class UniprMagazzinoClientExtensions
{
    public static IServiceCollection AddUniprMagazzinoClient(this IServiceCollection services, IConfiguration configuration)
    {
        IConfigurationSection confSection = configuration.GetSection(UniprMagazzinoClientOptions.SectionName);
        UniprMagazzinoClientOptions options = confSection.Get<UniprMagazzinoClientOptions>() ?? new();

        services.AddHttpClient<IMagazzinoClientHttp, MagazzinoClientHttp>(o => {
            o.BaseAddress = new Uri(options.BaseAddress);
        }).ConfigurePrimaryHttpMessageHandler(_ => new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; }
        });

        return services;
    }
}