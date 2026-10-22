using System.Globalization;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace Sci.Web.Helpers
{
    public static class Util
    {
        // Método que devuelve el valor de almacenado dentro de una claim del token
        public static string GetClaimValue(this IPrincipal currentPrincipal, string key)
        {
            var identity = currentPrincipal.Identity as ClaimsIdentity;
            if (identity == null)
                return null;

            var claim = identity.Claims.FirstOrDefault(c => c.Type == key);
            return claim.Value;
        }

        // Método que devuelve la ruta de una acción dentro de un controlador, ej: unidades/getUnidades
        public static string GetActionName(ControllerContext c)
        {
            
            string? controllerName = c.RouteData.Values["controller"]!.ToString();
            string? actionName = c.RouteData.Values["action"]!.ToString();
            
            return controllerName+"/"+actionName;
        }

        // Método que remueve acentos para comparar criterios de búsqueda
        public static string RemoveAccents(string text)
        {
            string formD = text.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();

            foreach (char ch in formD)
            {
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (uc != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(ch);
                }
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }
    }    
}