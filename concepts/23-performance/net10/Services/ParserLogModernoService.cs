using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using PerformanceDemo.Models;

namespace PerformanceDemo.Services
{
    public class ParserLogModernoService
    {
        private readonly List<string> _linhasLog;

        public ParserLogModernoService()
        {
            _linhasLog = new List<string>(1000);
            for (int i = 0; i < 1000; i++)
            {
                int idIp = 100 + (i % 150);
                int status = (i % 20 == 0) ? 500 : (i % 10 == 0 ? 404 : 200);
                int bytes = 1024 + (i * 17) % 8000;
                int duracao = 5 + (i % 45);
                _linhasLog.Add($"2026-09-14 11:45:12.345 [INFO] IP:192.168.1.{idIp} GET /api/produtos/categoria/eletronicos Status:{status} Bytes:{bytes} Duration:{duracao}ms");
            }
        }

        public ResultadoBenchmark BenchmarkSubstring(int totalLinhas)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            int gen0Antes = GC.CollectionCount(0);
            long bytesAntes = GC.GetAllocatedBytesForCurrentThread();
            var sw = Stopwatch.StartNew();

            int somaBytes = 0;
            int totalProcessado = 0;

            for (int i = 0; i < totalLinhas; i++)
            {
                string linha = _linhasLog[i % _linhasLog.Count];

                // Extração com Substring e Split (aloca strings e arrays na Heap)
                int idxStatus = linha.IndexOf("Status:");
                int idxBytes = linha.IndexOf("Bytes:");
                int idxDuration = linha.IndexOf("Duration:");

                if (idxStatus != -1 && idxBytes != -1 && idxDuration != -1)
                {
                    string strStatus = linha.Substring(idxStatus + 7, idxBytes - (idxStatus + 7)).Trim();
                    string strBytes = linha.Substring(idxBytes + 6, idxDuration - (idxBytes + 6)).Trim();

                    somaBytes += int.Parse(strBytes);
                    totalProcessado++;
                }
            }

            sw.Stop();
            long bytesDepois = GC.GetAllocatedBytesForCurrentThread();
            int gen0Depois = GC.CollectionCount(0);

            return new ResultadoBenchmark
            {
                Metodo = "Substring (Alocação Tradicional em Heap)",
                Operacao = "Parsing de Strings",
                ItensProcessados = totalProcessado,
                TempoMs = Math.Round(sw.Elapsed.TotalMilliseconds, 2),
                BytesAlocadosEstimados = bytesDepois - bytesAntes,
                ColetasGen0 = gen0Depois - gen0Antes,
                Descricao = "Cada Substring cria novos objetos string na Heap gerenciada, provocando pressão contínua e coletas na Geração 0 do GC."
            };
        }

        public ResultadoBenchmark BenchmarkSpan(int totalLinhas)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            int gen0Antes = GC.CollectionCount(0);
            long bytesAntes = GC.GetAllocatedBytesForCurrentThread();
            var sw = Stopwatch.StartNew();

            int somaBytes = 0;
            int totalProcessado = 0;

            for (int i = 0; i < totalLinhas; i++)
            {
                string linha = _linhasLog[i % _linhasLog.Count];
                ReadOnlySpan<char> span = linha.AsSpan();

                // Fatiamento de memória em pilha com ReadOnlySpan (Zero Allocation)
                int idxStatus = span.IndexOf("Status:".AsSpan());
                int idxBytes = span.IndexOf("Bytes:".AsSpan());
                int idxDuration = span.IndexOf("Duration:".AsSpan());

                if (idxStatus != -1 && idxBytes != -1 && idxDuration != -1)
                {
                    ReadOnlySpan<char> sliceStatus = span.Slice(idxStatus + 7, idxBytes - (idxStatus + 7)).Trim();
                    ReadOnlySpan<char> sliceBytes = span.Slice(idxBytes + 6, idxDuration - (idxBytes + 6)).Trim();

                    if (int.TryParse(sliceBytes, out int valorBytes))
                    {
                        somaBytes += valorBytes;
                        totalProcessado++;
                    }
                }
            }

            sw.Stop();
            long bytesDepois = GC.GetAllocatedBytesForCurrentThread();
            int gen0Depois = GC.CollectionCount(0);

            return new ResultadoBenchmark
            {
                Metodo = "ReadOnlySpan<char> (Zero-Allocation)",
                Operacao = "Parsing de Strings",
                ItensProcessados = totalProcessado,
                TempoMs = Math.Round(sw.Elapsed.TotalMilliseconds, 2),
                BytesAlocadosEstimados = bytesDepois - bytesAntes,
                ColetasGen0 = gen0Depois - gen0Antes,
                Descricao = "O fatiamento com Span opera exclusivamente na pilha (Stack), sem alocar nenhuma nova string na Heap e com coletas nulas no GC."
            };
        }

        public ResultadoBenchmark BenchmarkArrayNovo(int iteracoes, int tamanhoKb)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            int tamanhoBytes = tamanhoKb * 1024;
            int gen0Antes = GC.CollectionCount(0);
            long bytesAntes = GC.GetAllocatedBytesForCurrentThread();
            var sw = Stopwatch.StartNew();

            for (int i = 0; i < iteracoes; i++)
            {
                // Alocação constante de novos vetores
                byte[] buffer = new byte[tamanhoBytes];
                buffer[0] = (byte)(i % 255);
                buffer[tamanhoBytes - 1] = 0xAA;
            }

            sw.Stop();
            long bytesDepois = GC.GetAllocatedBytesForCurrentThread();
            int gen0Depois = GC.CollectionCount(0);

            return new ResultadoBenchmark
            {
                Metodo = "new byte[] (Alocação Direta por Requisição)",
                Operacao = "Gerenciamento de Buffers",
                ItensProcessados = iteracoes,
                TempoMs = Math.Round(sw.Elapsed.TotalMilliseconds, 2),
                BytesAlocadosEstimados = bytesDepois - bytesAntes,
                ColetasGen0 = gen0Depois - gen0Antes,
                Descricao = "A cada operação de I/O um novo array é alocado na Heap. Se o tamanho for maior que 85.000 bytes, atinge a LOH gerando fragmentação."
            };
        }

        public ResultadoBenchmark BenchmarkArrayPool(int iteracoes, int tamanhoKb)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            int tamanhoBytes = tamanhoKb * 1024;
            int gen0Antes = GC.CollectionCount(0);
            long bytesAntes = GC.GetAllocatedBytesForCurrentThread();
            var sw = Stopwatch.StartNew();

            var pool = ArrayPool<byte>.Shared;

            for (int i = 0; i < iteracoes; i++)
            {
                // Aluga do pool e devolve após o uso
                byte[] buffer = pool.Rent(tamanhoBytes);
                try
                {
                    buffer[0] = (byte)(i % 255);
                    buffer[tamanhoBytes - 1] = 0xAA;
                }
                finally
                {
                    pool.Return(buffer);
                }
            }

            sw.Stop();
            long bytesDepois = GC.GetAllocatedBytesForCurrentThread();
            int gen0Depois = GC.CollectionCount(0);

            return new ResultadoBenchmark
            {
                Metodo = "ArrayPool<byte>.Shared (Reciclagem de Buffers)",
                Operacao = "Gerenciamento de Buffers",
                ItensProcessados = iteracoes,
                TempoMs = Math.Round(sw.Elapsed.TotalMilliseconds, 2),
                BytesAlocadosEstimados = bytesDepois - bytesAntes,
                ColetasGen0 = gen0Depois - gen0Antes,
                Descricao = "O pool reutiliza os mesmos blocos de memória previamente alocados. As alocações em Heap caem para próximo de zero."
            };
        }
    }
}
