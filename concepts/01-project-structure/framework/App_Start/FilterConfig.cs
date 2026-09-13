using System.Web.Mvc;

namespace ProjectStructure
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            // Filtro global de tratamento de erro padrão do MVC
            filters.Add(new HandleErrorAttribute());
        }
    }
}
