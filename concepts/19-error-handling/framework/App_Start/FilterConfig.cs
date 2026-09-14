using System.Web.Mvc;

namespace ErrorHandlingDemo
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            // O filtro HandleErrorAttribute captura excecoes lancadas nas Actions do MVC
            // e redireciona automaticamente para a view ~/Views/Shared/Error.cshtml
            // apenas quando customErrors estiver com mode="On" ou "RemoteOnly" no Web.config.
            filters.Add(new HandleErrorAttribute());
        }
    }
}
