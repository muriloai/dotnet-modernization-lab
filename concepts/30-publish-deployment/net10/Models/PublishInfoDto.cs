namespace PublishDeployDemo.Models;

public record DiagnosticoRuntimeDto(
    string SistemaOperacional,
    string Arquitetura,
    string Framework,
    string VersaoRuntime,
    bool Is64BitProcess,
    double TempoAtividadeSegundos,
    double MemoriaAlocadaMb,
    string ModoExecucao
);

public record ComparativoPublicacaoDto(
    string Modo,
    string ComandoCli,
    string TamanhoAproximado,
    string TempoInicializacao,
    string Portabilidade,
    string Descricao
);

public record GeradorComandoRequest(
    string Rid,
    string Configuracao,
    bool SelfContained,
    bool SingleFile,
    bool ReadyToRun,
    bool EnableCompression
);

public record GeradorComandoResponse(
    string ComandoGerado,
    string DescricaoEstrategia,
    string ArtefatoFinalEsperado
);
