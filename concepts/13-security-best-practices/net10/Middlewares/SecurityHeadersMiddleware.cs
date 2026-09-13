namespace SecurityBestPracticesDemo.Middlewares;

/// <summary>
/// Middleware moderno para injeção programática de cabeçalhos de segurança HTTP.
/// Protege a aplicação contra ataques de Clickjacking, MIME-sniffing e Cross-Site Scripting (XSS).
/// </summary>
public sealed class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 1. Evita que o navegador tente adivinhar o MIME-type do conteúdo (MIME-sniffing)
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");

        // 2. Impede que a página seja carregada dentro de frames ou iframes (Proteção contra Clickjacking)
        context.Response.Headers.Append("X-Frame-Options", "DENY");

        // 3. Controla a quantidade de informações de referência enviadas em links externos
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

        // 4. Política de Segurança de Conteúdo (Content Security Policy - CSP)
        context.Response.Headers.Append(
            "Content-Security-Policy",
            "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'; img-src 'self' data:;"
        );

        // 5. Restringe acesso a recursos sensíveis do dispositivo
        context.Response.Headers.Append(
            "Permissions-Policy",
            "camera=(), microphone=(), geolocation=()"
        );

        await _next(context);
    }
}
