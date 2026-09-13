using System;
using System.Configuration;
using System.Web.Http;

namespace ProjectStructure.Controllers.Api
{
    /// <summary>
    /// Controller Web API 2 herdando de System.Web.Http.ApiController.
    /// No .NET Framework, repare que o namespace é diferente do controller MVC:
    /// MVC usava System.Web.Mvc enquanto Web API usava System.Web.Http!
    /// </summary>
    [RoutePrefix("api/info")]
    public class InfoApiController : ApiController
    {
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetInfo()
        {
            return Ok(new
            {
                framework = ".NET Framework 4.8.1",
                csharpVersion = "C# 7.3 (Máximo suportado)",
                hostingModel = "Windows IIS / IIS Express (Worker Process w3wp.exe)",
                projectFileFormat = "Old-style verbose XML (.csproj)",
                entryPoint = "Global.asax.cs (Application_Start)",
                configurationSource = "Web.config (<appSettings> e <connectionStrings>)",
                dependencyInjection = "Nenhum container nativo (exigia Autofac/Unity)",
                timestamp = DateTime.UtcNow
            });
        }
    }
}
