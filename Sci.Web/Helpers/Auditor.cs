using Sci.Datos;
using Sci.Entidades;

namespace Sci.Web.Helpers
{
    public static class Auditor
    {
        public static void registroAuditor(string accion, int status, string ip, int usuario, int rolid, string EndPoint, string mensaje, string parametros, DbContextUsuarios _cn)
        {
            auditor newAuditor = new auditor
            {
                SISTEMAID = 19,
                ACCION = accion,
                STATUSCODE = status,
                ENDPOINT = EndPoint,
                MESSAGE = mensaje.Length > 500 ? mensaje.Substring(0, 499) : mensaje,
                PARAMETROS = parametros,
                IP = ip,
                USUARIO = usuario,
                ROLID = rolid,
                CREACION = DateTime.Now
            };

            _cn.Auditor.Add(newAuditor);
            _cn.SaveChanges();
        }
    }    
}