using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using HttpClientDemo.Models;

namespace HttpClientDemo.Services
{
    /// <summary>
    /// Serviço legado demonstrando as três abordagens clássicas do .NET Framework 4.8.1:
    /// 1. using (var client = new HttpClient()) (Risco de Socket Exhaustion)
    /// 2. static readonly HttpClient (Risco de DNS Stale)
    /// 3. WebClient legado síncrono (Bloqueio de threads)
    /// </summary>
    public static class CotacaoLegadaService
    {
        // Instância estática reaproveitada para evitar esgotamento de portas efêmeras
        private static readonly HttpClient _staticClient = new HttpClient();

        /// <summary>
        /// Anti-padrão histórico: instanciar HttpClient dentro de bloco using.
        /// O descarte encerra a instância, mas a conexão TCP subjacente permanece
        /// em TIME_WAIT no sistema operacional por até 4 minutos.
        /// </summary>
        public static async Task<MetricaLegadaModel> ConsultarComUsingAsync(string moeda)
        {
            var sw = Stopwatch.StartNew();

            using (var client = new HttpClient())
            {
                var cotacao = SimularRespostaLocal(moeda);
                await Task.Delay(25); // Simula latência de rede
                sw.Stop();

                return new MetricaLegadaModel
                {
                    ModoCliente = "using (var client = new HttpClient()) - Anti-padrão Legado",
                    TempoRespostaMs = sw.ElapsedMilliseconds,
                    StatusCode = 200,
                    Mensagem = "Instância descartada pelo Dispose(), mas o socket TCP permanece em estado TIME_WAIT no SO.",
                    Dados = cotacao
                };
            }
        }

        /// <summary>
        /// Abordagem de mitigação legada com HttpClient estático.
        /// Resolve o problema de Socket Exhaustion, mas não atualiza alterações de DNS (DNS Stale).
        /// </summary>
        public static async Task<MetricaLegadaModel> ConsultarComStaticAsync(string moeda)
        {
            var sw = Stopwatch.StartNew();

            var cotacao = SimularRespostaLocal(moeda);
            await Task.Delay(10);
            sw.Stop();

            return new MetricaLegadaModel
            {
                ModoCliente = "static readonly HttpClient - Mitigação Parcial",
                TempoRespostaMs = sw.ElapsedMilliseconds,
                StatusCode = 200,
                Mensagem = "Reaproveita a conexão TCP, mas nunca revalida alterações de IP no servidor DNS.",
                Dados = cotacao
            };
        }

        /// <summary>
        /// Classe WebClient legada do .NET 2.0/4.0 com chamadas síncronas bloqueantes.
        /// </summary>
        public static MetricaLegadaModel ConsultarComWebClient(string moeda)
        {
            var sw = Stopwatch.StartNew();

            using (var client = new WebClient())
            {
                var cotacao = SimularRespostaLocal(moeda);
                System.Threading.Thread.Sleep(30); // Simula bloqueio de thread
                sw.Stop();

                return new MetricaLegadaModel
                {
                    ModoCliente = "WebClient Legado (Síncrono)",
                    TempoRespostaMs = sw.ElapsedMilliseconds,
                    StatusCode = 200,
                    Mensagem = "Execução síncrona bloqueando threads da thread pool do IIS.",
                    Dados = cotacao
                };
            }
        }

        private static CotacaoLegadaModel SimularRespostaLocal(string moeda)
        {
            decimal valor = 5.65m;
            string nome = "Dólar Comercial";

            if (string.Equals(moeda, "EUR", StringComparison.OrdinalIgnoreCase))
            {
                valor = 6.15m;
                nome = "Euro";
            }
            else if (string.Equals(moeda, "GBP", StringComparison.OrdinalIgnoreCase))
            {
                valor = 7.20m;
                nome = "Libra Esterlina";
            }

            return new CotacaoLegadaModel
            {
                Moeda = moeda.ToUpperInvariant(),
                Nome = nome,
                ValorReais = valor,
                VariacaoPercentual = 0.35m,
                DataHora = DateTime.Now,
                Fonte = "Legado Central Bank Simulator (.NET Framework 4.8.1)"
            };
        }
    }
}
