using System.Text.Json;
using SecurityBestPracticesDemo.Models;

namespace SecurityBestPracticesDemo.Services;

/// <summary>
/// Contrato do serviço de transferências e auditoria de segurança.
/// </summary>
public interface ITransferenciaService
{
    Task<TransacaoRegistro> ExecutarTransferenciaAsync(TransferenciaRequest request);
    IEnumerable<TransacaoRegistro> ListarTransacoes();
    string SerializarTransacaoSegura(TransacaoRegistro transacao);
    TransacaoRegistro? DesserializarTransacaoSegura(string json);
}
