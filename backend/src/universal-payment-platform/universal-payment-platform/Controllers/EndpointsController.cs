using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using System.Collections.Concurrent;

namespace universal_payment_platform.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "SuperAdmin,Developer,Admin")] // Only allow authorized developers/admins
    public class EndpointsController(IApiDescriptionGroupCollectionProvider provider) : ControllerBase
    {
        private readonly IApiDescriptionGroupCollectionProvider _provider = provider;
        private static readonly ConcurrentDictionary<string, List<EndpointInfo>> _cachedEndpoints = new();

        [HttpGet]
        public IActionResult GetAll()
        {
            const string cacheKey = "AllEndpoints";

            // Return cached result if available
            if (_cachedEndpoints.TryGetValue(cacheKey, out var cached))
            {
                return Ok(cached);
            }

            // Build typed endpoints list
            var endpoints = _provider.ApiDescriptionGroups.Items
                .Where(g => g != null)
                .SelectMany(g => g.Items ?? Enumerable.Empty<ApiDescription>())
                .Where(api => !string.IsNullOrWhiteSpace(api?.RelativePath))
                .Where(api => !api!.RelativePath!.Contains("swagger", StringComparison.OrdinalIgnoreCase)
                           && !api.RelativePath.Contains("health", StringComparison.OrdinalIgnoreCase))
                .Select(api => new EndpointInfo
                {
                    HttpMethod = api.HttpMethod ?? "ANY",
                    Route = "/" + api.RelativePath!.Trim('/').ToLowerInvariant(),
                    Controller = api.ActionDescriptor?.RouteValues?.TryGetValue("controller", out var c) == true ? c ?? "N/A" : "N/A",
                    Action = api.ActionDescriptor?.RouteValues?.TryGetValue("action", out var a) == true ? a ?? "N/A" : "N/A",
                    Parameters = (api.ParameterDescriptions ?? Enumerable.Empty<ApiParameterDescription>())
                        .Select(p => new ParameterInfo
                        {
                            Name = p.Name ?? "unknown",
                            Type = p.Type?.Name ?? "unknown",
                            Source = p.Source?.ToString() ?? "unknown"
                        }).ToList()
                })
                .OrderBy(e => e.Route)
                .ToList();

            // Cache result for subsequent requests
            _cachedEndpoints[cacheKey] = endpoints;

            return Ok(endpoints);
        }
    }

    // Typed DTOs
    public class EndpointInfo
    {
        public string HttpMethod { get; set; } = "";
        public string Route { get; set; } = "";
        public string Controller { get; set; } = "";
        public string Action { get; set; } = "";
        public List<ParameterInfo> Parameters { get; set; } = new();
    }

    public class ParameterInfo
    {
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public string Source { get; set; } = "";
    }
}
