using ErrorHandlingDemo.Handlers;

var builder = WebApplication.CreateBuilder(args);

// Ativa suporte nativo a ProblemDetails (RFC 7807 e RFC 9457)
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Instance = context.HttpContext.Request.Path;
        context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
        context.ProblemDetails.Extensions["timestamp"] = DateTime.UtcNow;
    };
});

// Encadeamento ordenado de IExceptionHandler (Chain of Responsibility)
builder.Services.AddExceptionHandler<EntidadeNaoEncontradaExceptionHandler>();
builder.Services.AddExceptionHandler<RegraNegocioExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddControllers();

var app = builder.Build();

// Middleware centralizado unificado de tratamento de excecoes
app.UseExceptionHandler();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

app.Run();
