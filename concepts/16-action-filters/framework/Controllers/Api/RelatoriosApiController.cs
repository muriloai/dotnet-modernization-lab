using System;
using System.Threading.Tasks;
using System.Web.Http;
using ActionFiltersDemo.Filters;

namespace ActionFiltersDemo.Controllers.Api
{
    /// <summary>
    /// Controller da Web API 2 demonstrando a aplicação de filtros legados por atributo.
    /// </summary>
    [RoutePrefix("api/relatorios")]
    public class RelatoriosApiController : ApiController
    {
        [HttpGet]
        [Route("publico")]
        [MedirTempoExecucaoFilter]
        public IHttpActionResult ObterRelatorioPublico()
        {
            return Ok(new
            {
                Tipo = "Relatório Público de Vendas (Legado)",
                TotalVendas = 9870,
                Status = "Sucesso",
                Mensagem = "Interceptado pelo MedirTempoExecucaoFilter."
            });
        }

        [HttpGet]
        [Route("financeiro-protegido")]
        [ValidarApiKeyLegado]
        [MedirTempoExecucaoFilter]
        public IHttpActionResult ObterRelatorioFinanceiroProtegido()
        {
            return Ok(new
            {
                Tipo = "Relatório Financeiro Confidencial (Legado)",
                FaturamentoBruto = 3950000.00m,
                LucroLiquido = 890200.00m,
                MargemOperacional = "22.5%",
                DataFechamento = DateTime.Now.ToString("yyyy-MM-dd"),
                Autorizacao = "Chave validada via ValidarApiKeyLegadoAttribute."
            });
        }

        [HttpGet]
        [Route("processamento-pesado")]
        [MedirTempoExecucaoFilter]
        public async Task<IHttpActionResult> ObterProcessamentoPesado()
        {
            await Task.Delay(120);

            return Ok(new
            {
                Tipo = "Cálculo de Projeção Anual (Legado)",
                Status = "Concluído após 120ms de processamento assíncrono.",
                Instrucao = "Inspecione o cabeçalho 'X-Tempo-Execucao-Ms' na resposta HTTP."
            });
        }
    }
}
