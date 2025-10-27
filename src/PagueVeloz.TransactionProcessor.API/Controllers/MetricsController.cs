using Microsoft.AspNetCore.Mvc;
using PagueVeloz.TransactionProcessor.Application.Services;

namespace PagueVeloz.TransactionProcessor.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MetricsController : ControllerBase
{
    private readonly IMetricsService _metricsService;

    public MetricsController(IMetricsService metricsService)
    {
        _metricsService = metricsService;
    }

    [HttpGet]
    public ActionResult<Dictionary<string, object>> GetMetrics()
    {
        var metrics = _metricsService.GetMetrics();
        return Ok(metrics);
    }
}

