namespace Sci.Entidades
{
    public class auditor
    {
        public int ID { get; set; }
        public int SISTEMAID { get; set; }
        public string? ACCION { get; set; }
        public string? PARAMETROS { get; set; }
        public int STATUSCODE { get; set; }
        public string? IP { get; set; }
        public int USUARIO { get; set; }
        public DateTime CREACION { get; set; }
        public int ROLID { get; set; }
        public string? ENDPOINT { get; set; }
        public string? MESSAGE { get; set; }
    }
}
