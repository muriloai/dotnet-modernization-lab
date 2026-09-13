using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using SecurityBestPracticesDemo.Models;

namespace SecurityBestPracticesDemo.Services
{
    /// <summary>
    /// Serviço demonstrativo do padrão legado com BinaryFormatter.
    /// Exibe a sintaxe clássica e documenta as razões técnicas que motivaram seu banimento no .NET moderno.
    /// </summary>
    public static class SerializadorLegadoService
    {
        public static byte[] SerializarParaBytes(ObjetoSessaoLegado objeto)
        {
            var formatter = new BinaryFormatter();
            using (var stream = new MemoryStream())
            {
                formatter.Serialize(stream, objeto);
                return stream.ToArray();
            }
        }

        public static ObjetoSessaoLegado DesserializarDeBytes(byte[] bytes)
        {
            var formatter = new BinaryFormatter();
            using (var stream = new MemoryStream(bytes))
            {
                // Risco de RCE: O BinaryFormatter recria grafos arbitrários de objetos a partir dos metadados no stream.
                return (ObjetoSessaoLegado)formatter.Deserialize(stream);
            }
        }
    }
}
