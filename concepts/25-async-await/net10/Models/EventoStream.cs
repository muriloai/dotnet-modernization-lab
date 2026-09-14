using System;

namespace AsyncAwaitDemo.Models;

public record EventoStream(
    int Id,
    string Descricao,
    DateTime Timestamp,
    int LatenciaMs
);
