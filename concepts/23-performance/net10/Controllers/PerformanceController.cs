using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using PerformanceDemo.Models;
using PerformanceDemo.Services;

namespace PerformanceDemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PerformanceController : ControllerBase
    {
        private readonly ParserLogModernoService _service;

        public PerformanceController(ParserLogModernoService service)
        {
            _service = service;
        }

        [HttpPost("comparar-parsing")]
        public ActionResult<List<ResultadoBenchmark>> CompararParsing([FromQuery] int linhas = 50000)
        {
            if (linhas < 1000 || linhas > 500000)
            {
                linhas = 50000;
            }

            var resultadoSubstring = _service.BenchmarkSubstring(linhas);
            var resultadoSpan = _service.BenchmarkSpan(linhas);

            return Ok(new List<ResultadoBenchmark> { resultadoSubstring, resultadoSpan });
        }

        [HttpPost("comparar-buffers")]
        public ActionResult<List<ResultadoBenchmark>> CompararBuffers([FromQuery] int iteracoes = 15000, [FromQuery] int tamanhoKb = 64)
        {
            if (iteracoes < 1000 || iteracoes > 100000)
            {
                iteracoes = 15000;
            }

            if (tamanhoKb < 1 || tamanhoKb > 1024)
            {
                tamanhoKb = 64;
            }

            var resultadoArrayNovo = _service.BenchmarkArrayNovo(iteracoes, tamanhoKb);
            var resultadoArrayPool = _service.BenchmarkArrayPool(iteracoes, tamanhoKb);

            return Ok(new List<ResultadoBenchmark> { resultadoArrayNovo, resultadoArrayPool });
        }
    }
}
