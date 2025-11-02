using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using System.Collections.Concurrent;

namespace MyApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Developer,Admin")] // Only allow authorized developers/admins
    public class EndpointsController : ControllerBase
    {
        private readonly IApiDescriptionGroupCollectionProvider _provider;
        private static readonly ConcurrentDictionary<string, object> _cachedEndpoints = new();

        public EndpointsController(IApiDescriptionGroupCollectionProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            const string cacheKey = "AllEndpoints";

            // Return cached result if available
            if (_cachedEndpoints.TryGetValue(cacheKey, out var cached))
            {
                return Ok(cached);
            }

            // Build endpoints list
            var endpoints = _provider.ApiDescriptionGroups.Items
                .Where(g => g != null)
                .SelectMany(g => g.Items ?? Enumerable.Empty<ApiDescription>())
                .Where(api => !string.IsNullOrWhiteSpace(api?.RelativePath))
                .Where(api => !api!.RelativePath!.Contains("swagger", StringComparison.OrdinalIgnoreCase)
                           && !api.RelativePath.Contains("health", StringComparison.OrdinalIgnoreCase))
                .Select(api => new
                {
                    HttpMethod = api.HttpMethod ?? "ANY",
                    Route = "/" + api.RelativePath!.Trim('/'),
                    Controller = api.ActionDescriptor?.RouteValues?.TryGetValue("controller", out var c) == true ? c ?? "N/A" : "N/A",
                    Action = api.ActionDescriptor?.RouteValues?.TryGetValue("action", out var a) == true ? a ?? "N/A" : "N/A",
                    Parameters = (api.ParameterDescriptions ?? Enumerable.Empty<ApiParameterDescription>())
                        .Select(p => new
                        {
                            Name = p.Name ?? "unknown",
                            Type = p.Type?.Name ?? "unknown",
                            Source = p.Source?.ToString() ?? "unknown"
                        })
                })
                .OrderBy(e => e.Route)
                .ToList();

            // Cache result for subsequent requests
            _cachedEndpoints[cacheKey] = endpoints;

            return Ok(endpoints);
        }
    }
}
