namespace AgriActifs.Web.Services.Interfaces;

public record DiscoveredMvcEndpoint(
    string? Area,
    string Controller,
    string Action,
    string HttpMethod,
    bool IsMapped);

public interface IMvcEndpointScanner
{
    Task<IReadOnlyList<DiscoveredMvcEndpoint>> DiscoverAllAsync(CancellationToken cancellationToken = default);
}
