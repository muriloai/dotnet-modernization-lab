using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecksDemo.Formatters;

public static class HealthCheckResponseWriters
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static async Task EscreverRespostaJson(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json; charset=utf-8";

        var payload = new
        {
            statusGeral = report.Status.ToString(),
            duracaoTotalMs = Math.Round(report.TotalDuration.TotalMilliseconds, 2),
            dataHora = DateTime.UtcNow,
            verificacoes = report.Entries.Select(e => new
            {
                nome = e.Key,
                status = e.Value.Status.ToString(),
                duracaoMs = Math.Round(e.Value.Duration.TotalMilliseconds, 2),
                descricao = e.Value.Description,
                tags = e.Value.Tags,
                dados = e.Value.Data.Count > 0 ? e.Value.Data : null,
                erro = e.Value.Exception?.Message
            })
        };

        await JsonSerializer.SerializeAsync(context.Response.Body, payload, JsonOptions);
    }
}
