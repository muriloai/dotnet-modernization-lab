using LoggingDemo.Services;
using Microsoft.AspNetCore.Mvc;

namespace LoggingDemo.Controllers;

[ApiController]
[Route("api/logs-inspecao")]
public class LogsInspecaoController : ControllerBase
{
    private readonly IProvedorLogsMemoria _provedor;

    public LogsInspecaoController(IProvedorLogsMemoria provedor)
    {
        _provedor = provedor;
    }

    [HttpGet]
    public IActionResult ObterLogs()
    {
        var logs = _provedor.ObterLogs();
        return Ok(logs);
    }

    [HttpDelete]
    public IActionResult LimparLogs()
    {
        _provedor.Limpar();
        return NoContent();
    }
}
