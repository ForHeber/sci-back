using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sci.Entidades
{
    [Table("PERSONA")]//es necesario definir la vista como tabla para poder generar relaciones con EF
    public class persona
    {
        [Key]//es necesario determinar la "llave primaria" de la vista
        public int RUN { get; set; }
        public int COD_CATEGORIA { get; set; }
        public string? GRADO { get; set; }
        public int COD_GRADO { get; set; }
        public string? ESFON { get; set; }
        public int COD_ESCALAFON { get; set; }
        public string? APELLIDO_PATERNO { get; set; }
        public string? APELLIDO_MATERNO { get; set; }
        public string? NOMBRES { get; set; }
        public string? NOMBRE { get; set; }
        public string? UNIDAD { get; set; }
        public string? UNIDADCODIGO { get; set; }
        public string? FONO { get; set; }
        public string? MOVIL { get; set; }
        public string? CORREOINTRA { get; set; }
        public string? CORREOINTER { get; set; }
        public float PESO { get; set; }
        public float ESTATURA { get; set; }
        public string? COD_SEXO { get; set; }
        public string? ESTADO_CIVIL { get; set; }
        public string? GRUPO_SANGRE { get; set; }
        public int EDAD { get; set; }
        public string? ESTADO_OP { get; set; }
        public string? ESTADO_OP_EX { get; set; }
        // public string? PASSWORD { get; set; }
        public string? FOTO { get; set; }
        public string? FOTO_TN { get; set; }
        public int ORDEN { get; set; }
    }
}
