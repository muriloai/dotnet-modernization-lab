using System.Diagnostics;
using System.Text.Json;
using JsonSerializationDemo.Contexts;
using JsonSerializationDemo.Models;

namespace JsonSerializationDemo.Services;

public class SerializacaoBenchmarkService
{
    private static readonly JsonSerializerOptions OptionsReflexao = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public MetricasSerializacao ExecutarBenchmark(int quantidade = 2500)
    {
        var produtos = GerarProdutos(quantidade);

        // Aquecimento (Warm-up) para carregar tipos no JIT
        _ = JsonSerializer.Serialize(produtos.Take(10), OptionsReflexao);
        _ = JsonSerializer.SerializeToUtf8Bytes(produtos.Take(10).ToList(), CatalogoJsonContext.Default.ListProdutoCatalogo);

        // 1. Teste via Reflexao Tradicional
        long memAntesReflexao = GC.GetAllocatedBytesForCurrentThread();
        var swReflexao = Stopwatch.StartNew();
        for (int i = 0; i < 5; i++)
        {
            _ = JsonSerializer.Serialize(produtos, OptionsReflexao);
        }
        swReflexao.Stop();
        long memDepoisReflexao = GC.GetAllocatedBytesForCurrentThread();
        long alocacaoReflexao = memDepoisReflexao - memAntesReflexao;

        // 2. Teste via Source Generator (Zero Reflexao e UTF-8 Direto)
        long memAntesSourceGen = GC.GetAllocatedBytesForCurrentThread();
        var swSourceGen = Stopwatch.StartNew();
        for (int i = 0; i < 5; i++)
        {
            _ = JsonSerializer.SerializeToUtf8Bytes(produtos, CatalogoJsonContext.Default.ListProdutoCatalogo);
        }
        swSourceGen.Stop();
        long memDepoisSourceGen = GC.GetAllocatedBytesForCurrentThread();
        long alocacaoSourceGen = memDepoisSourceGen - memAntesSourceGen;

        double tempoReflexaoMs = swReflexao.Elapsed.TotalMilliseconds;
        double tempoSourceGenMs = swSourceGen.Elapsed.TotalMilliseconds;

        double ganhoTempo = tempoReflexaoMs > 0
            ? Math.Round(((tempoReflexaoMs - tempoSourceGenMs) / tempoReflexaoMs) * 100, 2)
            : 0;

        double reducaoMemoria = alocacaoReflexao > 0
            ? Math.Round(((double)(alocacaoReflexao - alocacaoSourceGen) / alocacaoReflexao) * 100, 2)
            : 0;

        return new MetricasSerializacao
        {
            TotalItens = quantidade * 5,
            TempoReflexaoMs = Math.Round(tempoReflexaoMs, 2),
            TempoSourceGenMs = Math.Round(tempoSourceGenMs, 2),
            GanhoPercentualTempo = Math.Max(0, ganhoTempo),
            MemoriaAlocadaReflexaoBytes = alocacaoReflexao,
            MemoriaAlocadaSourceGenBytes = alocacaoSourceGen,
            ReducaoMemoriaPercentual = Math.Max(0, reducaoMemoria)
        };
    }

    public List<ProdutoCatalogo> GerarProdutos(int quantidade)
    {
        var lista = new List<ProdutoCatalogo>(quantidade);
        for (int i = 1; i <= quantidade; i++)
        {
            lista.Add(new ProdutoCatalogo(
                i,
                $"Componente de Servidor Enterprise #{i:D5}",
                150.00m + (i * 0.75m),
                i % 2 == 0 ? "Hardware" : "Acessórios",
                20 + (i % 50)));
        }
        return lista;
    }
}
