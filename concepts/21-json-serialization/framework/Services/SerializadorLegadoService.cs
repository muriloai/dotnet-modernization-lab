using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Web.Script.Serialization;
using JsonSerializationDemo.Models;
using Newtonsoft.Json;

namespace JsonSerializationDemo.Services
{
    public class SerializadorLegadoService
    {
        public List<ProdutoLegado> GerarLote(int quantidade)
        {
            var lista = new List<ProdutoLegado>(quantidade);
            for (int i = 1; i <= quantidade; i++)
            {
                lista.Add(new ProdutoLegado(
                    i,
                    "Produto Legado Enterprise #" + i.ToString("D5"),
                    100.00m + (i * 0.50m),
                    i % 2 == 0 ? "Hardware" : "Software",
                    50 + (i % 20)));
            }
            return lista;
        }

        public string SerializarComJavaScriptSerializer(object objeto, out double tempoMs)
        {
            var serializer = new JavaScriptSerializer();
            var sw = Stopwatch.StartNew();
            string json = serializer.Serialize(objeto);
            sw.Stop();
            tempoMs = sw.Elapsed.TotalMilliseconds;
            return json;
        }

        public string SerializarComNewtonsoft(object objeto, out double tempoMs)
        {
            var sw = Stopwatch.StartNew();
            string json = JsonConvert.SerializeObject(objeto, Formatting.Indented);
            sw.Stop();
            tempoMs = sw.Elapsed.TotalMilliseconds;
            return json;
        }
    }
}
