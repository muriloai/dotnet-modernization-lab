namespace HybridCacheDemo.Models;

public record ProductDto(
    int Id,
    string Nome,
    string Categoria,
    decimal Preco,
    int Estoque,
    DateTime DataOrigem
);

public record CacheResultDto<T>(
    T Dados,
    string Origem,
    long TempoExecucaoMs,
    long ContadorConsultasBanco,
    string Mensagem
);

public record StampedeTestResult(
    int TotalRequisicoes,
    int ExecucoesFactory,
    int TotalSucessos,
    long TempoTotalMs,
    string Diagnostico
);
