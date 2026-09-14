using System;
using System.Collections.Generic;
using System.Diagnostics;
using PerformanceDemo.Models;

namespace PerformanceDemo.Services
{
    public class ParserLogLegadoService
    {
        private readonly List<string> _linhasLog;

        public ParserLogLegadoService()
        {
            _linhasLog = new List<string>(1000);
            for (int i = 0; i < 1000; i++)
            {
                int idIp = 100 + (i % 150);
                int status = (i % 20 == 0) ? 500 : (i % 10 == 0 ? 404 : 200);
                int bytes = 1024 + (i * 17) % 8000;
                int duracao = 5 + (i % 45);
                _linhasLog.Add(string.Format("2026-09-14 11:45:12.345 [INFO] IP:192.168.1.{0} GET /api/produtos/categoria/eletronicos Status:{1} Bytes:{2} Duration:{3}ms", idIp, status, bytes, duracao));
            }
        }

        public ResultadoBenchmarkLegado ExecutarParsingComSubstring(int totalLinhas)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            int gen0Antes = GC.CollectionCount(0);
            long memAntes = GC.GetTotalMemory(false);
            var sw = Stopwatch.StartNew();

            int somaBytes = 0;
            int totalProcessado = 0;

            for (int i = 0; i < totalLinhas; i++)
            {
                string linha = _linhasLog[i % _linhasLog.Count];

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
            long memDepois = GC.GetTotalMemory(false);
            int gen0Depois = GC.CollectionCount(0);

            return new ResultadoBenchmarkLegado
            {
                Metodo = "string.Substring (Alocação Tradicional em Heap)",
                TotalProcessado = totalProcessado,
                TempoMs = Math.Round(sw.Elapsed.TotalMilliseconds, 2),
                MemoriaEstimadaBytes = Math.Max(0, memDepois - memAntes),
                ColetasGen0 = gen0Depois - gen0Antes
            };
        }

        public ResultadoBenchmarkLegado ExecutarAlocacaoBuffers(int iteracoes, int tamanhoKb)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            int tamanhoBytes = tamanhoKb * 1024;
            int gen0Antes = GC.CollectionCount(0);
            long memAntes = GC.GetTotalMemory(false);
            var sw = Stopwatch.StartNew();

            for (int i = 0; i < iteracoes; i++)
            {
                // Alocação repetitiva de novos vetores sem reaproveitamento
                byte[] buffer = new byte[tamanhoBytes];
                buffer[0] = (byte)(i % 255);
                buffer[tamanhoBytes - 1] = 0xAA;
            }

            sw.Stop();
            long memDepois = GC.GetTotalMemory(false);
            int gen0Depois = GC.CollectionCount(0);

            return new ResultadoBenchmarkLegado
            {
                Metodo = "new byte[] (Alocação Contínua sem Pool)",
                TotalProcessado = iteracoes,
                TempoMs = Math.Round(sw.Elapsed.TotalMilliseconds, 2),
                MemoriaEstimadaBytes = Math.Max(0, memDepois - memAntes),
                ColetasGen0 = gen0Depois - gen0Antes
            };
        }
    }
}
