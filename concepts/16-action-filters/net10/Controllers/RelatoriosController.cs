using Microsoft.AspNetCore.Mvc;
using ActionFiltersDemo.Filters;

namespace ActionFiltersDemo.Controllers;

/// <summary>
/// Controller didático demonstrando a aplicação prática de Action Filters modernos:
/// - [TypeFilter(typeof(TempoExecucaoActionFilter))]: Mede a latência e injeta cabeçalho HTTP.
/// - [ServiceFilter(typeof(ValidarApiKeyServiceFilter))]: Valida segurança com DI nativa e curto-circuito.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class RelatoriosController : ControllerBase
{
    /// <summary>
    /// Relatório público interceptado apenas pelo filtro de medição de tempo de execução.
    /// </summary>
    [HttpGet("publico")]
    [TypeFilter(typeof(TempoExecucaoActionFilter))]
    public IActionResult ObterRelatorioPublico()
    {
        return Ok(new
        {
            Tipo = "Relatório Público de Vendas",
            TotalVendas = 12450,
            Status = "Operação concluída com sucesso.",
            Mensagem = "Este endpoint foi interceptado pelo TempoExecucaoActionFilter."
        });
    }

    /// <summary>
    /// Relatório corporativo confidencial protegido por [ServiceFilter].
    /// Se o cabeçalho 'X-API-Key' não for enviado corretamente, o filtro bloqueia a execução com 401.
    /// </summary>
    [HttpGet("financeiro-protegido")]
    [ServiceFilter(typeof(ValidarApiKeyServiceFilter))]
    [TypeFilter(typeof(TempoExecucaoActionFilter))]
    public IActionResult ObterRelatorioFinanceiroProtegido()
    {
        return Ok(new
        {
            Tipo = "Relatório Financeiro Confidencial",
            FaturamentoBruto = 4589000.00m,
            LucroLiquido = 1120400.50m,
            MargemOperacional = "24.4%",
            DataFechamento = DateTime.UtcNow.ToString("yyyy-MM-dd"),
            Autorizacao = "Chave de API validada com sucesso via ValidarApiKeyServiceFilter com DI!"
        });
    }

    /// <summary>
    /// Ação que simula uma consulta pesada ao banco de dados ou cálculo demorado
    /// para destacar a medição de tempo pelo filtro de performance.
    /// </summary>
    [HttpGet("processamento-pesado")]
    [TypeFilter(typeof(TempoExecucaoActionFilter))]
    public async Task<IActionResult> ObterProcessamentoPesado()
    {
        // Simula operação com latência de processamento
        await Task.Delay(120);

        return Ok(new
        {
            Tipo = "Cálculo de Projeção Anual de Crescimento",
            Status = "Processamento finalizado após latência simulada de 120ms.",
            Instrucao = "Inspecione o cabeçalho HTTP de resposta 'X-Tempo-Execucao-Ms' para observar o tempo total registrado pelo filtro."
        });
    }
}
