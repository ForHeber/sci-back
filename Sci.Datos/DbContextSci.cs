using Microsoft.EntityFrameworkCore;
using Sci.Datos.Mapping;
using Sci.Entidades;

namespace Sci.Datos
{
    public class DbContextSci : DbContext
    {
        //Diccionario de datos
        public DbSet<persona> Personas { get; set; } = null!;
        public DbSet<unidad> Unidades { get; set; } = null!;
        public DbSet<estructura_superior> EstructurasSuperiores { get; set; } = null!;
        
        // Tablas Masters de Sistema.
        // Tablas Informes de Preliminares con sus Antecedentes relacionados
        public DbSet<preliminares> Preliminares { get; set; } = null!;
        public DbSet<involucrados_pre> InvolucradosPre { get; set; } = null!;
        public DbSet<antecedentes_judiciales_pre> AntecedentesJudicialesPre { get; set; } = null!;
        public DbSet<procedimientos_policiales_pre> ProcedimientosPolicialesPre { get; set; } = null!;
        public DbSet<archivos_informes_pre> ArchivosInformesPre { get; set; } = null!;

        // Tablas Informes de Incidentes con sus Antecedentes relacionados
        public DbSet<incidentes> Incidentes { get; set; } = null!;
        public DbSet<involucrados_inc> InvolucradosInc { get; set; } = null!;
        public DbSet<antecedentes_judiciales_inc> AntecedentesJudicialesInc { get; set; } = null!;
        public DbSet<procedimientos_policiales_inc> ProcedimientosPolicialesInc { get; set; } = null!;
        public DbSet<archivos_informes_inc> ArchivosInformesInc { get; set; } = null!;
        public DbSet<ampliaciones_informes_inc> AmpliacionesInformesInc { get; set; } = null!;
        public DbSet<motivo_devolucion_inc> MotivoDevolucionInc { get; set; } = null!;

        // Tablas Informes de Finales con sus Antecedentes relacionados
        public DbSet<finales> Finales { get; set; } = null!;
        public DbSet<involucrados_fin> InvolucradosFin { get; set; } = null!;
        public DbSet<antecedentes_judiciales_fin> AntecedentesJudicialesFin { get; set; } = null!;
        public DbSet<procedimientos_policiales_fin> ProcedimientosPolicialesFin { get; set; } = null!;
        public DbSet<archivos_informes_fin> ArchivosInformesFin { get; set; } = null!;
        public DbSet<ampliaciones_informes_fin> AmpliacionesInformesFin { get; set; } = null!;
        public DbSet<medidas_adoptadas_fin> MedidasAdoptadasFin { get; set; } = null!;
        public DbSet<motivo_devolucion_fin> MotivoDevolucionFin { get; set; } = null!;

        // Tablas de apoyo para incidentes (Referencias)
        public DbSet<estado_salud> EstadoSalud { get; set; } = null!;
        public DbSet<condicion_policial> CondicionPolicial { get; set; } = null!;
        public DbSet<tipo_armamento> TipoArmamento { get; set; } = null!;
        public DbSet<tipo_consumo> TipoConsumo { get; set; } = null!;
        public DbSet<tipo_identificacion> TipoIdentificacion { get; set; } = null!;
        public DbSet<tipo_grupo_incidente> TipoGrupoIncidente { get; set; } = null!;
        public DbSet<tipo_incidente> TipoIncidente { get; set; } = null!;
        public DbSet<tipo_interviniente> TipoInterviniente { get; set; } = null!;
        public DbSet<tipo_investigacion> TipoInvestigacion { get; set; } = null!;
        public DbSet<tipo_ingesta> TipoIngesta { get; set; } = null!;
        public DbSet<persona_sci> PersonaSci { get; set; } = null!;
        public DbSet<personas_civiles> PersonasCiviles { get; set; } = null!;
        public DbSet<indices_alertas> IndicesAlertas { get; set; } = null!;
        public DbSet<medidas_adoptadas> MedidasAdoptadas { get; set; } = null!;

        // Tabla de registro de eventos
        public DbSet<registro_eventos> RegistroEventos { get; set; } = null!;

        public DbContextSci(DbContextOptions<DbContextSci> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            //Diccionario de datos
            modelBuilder.ApplyConfiguration(new PersonaMap());
            modelBuilder.ApplyConfiguration(new UnidadMap());
            modelBuilder.ApplyConfiguration(new EstructuraSuperiorMap());

            // Crea variable navegacion U_INVOLUCRADO para relacionar campo UNIDAD_INVOLUCRADO de entidad involucrados_pre con CODIGO de la entidad unidad
            modelBuilder.Entity<involucrados_pre>().HasOne(s => s.U_INVOLUCRADO).WithMany().HasPrincipalKey(u => u.CODIGO).HasForeignKey(s => s.UNIDAD_INVOLUCRADO);
            // Crea variable navegacion U_INVOLUCRADO para relacionar campo UNIDAD_INVOLUCRADO de entidad involucrados_inc con CODIGO de la entidad unidad
            modelBuilder.Entity<involucrados_inc>().HasOne(s => s.U_INVOLUCRADO).WithMany().HasPrincipalKey(u => u.CODIGO).HasForeignKey(s => s.UNIDAD_INVOLUCRADO);
            // Crea variable navegacion U_INFORMANTE para relacionar campo UNIDAD_INFORMANTE de entidad preliminares con CODIGO de la entidad unidad
            modelBuilder.Entity<preliminares>().HasOne(s => s.U_INFORMANTE).WithMany().HasPrincipalKey(u => u.CODIGO).HasForeignKey(s => s.UNIDAD_INFORMANTE);
            // Crea variable navegacion U_AFECTADA para relacionar campo UNIDAD_AFECTADA de entidad preliminares con CODIGO de la entidad unidad
            modelBuilder.Entity<preliminares>().HasOne(s => s.U_AFECTADA).WithMany().HasPrincipalKey(u => u.CODIGO).HasForeignKey(s => s.UNIDAD_AFECTADA);
            // Crea variable navegacion U_INFORMANTE para relacionar campo UNIDAD_INFORMANTE de entidad incidentes con CODIGO de la entidad unidad
            modelBuilder.Entity<incidentes>().HasOne(s => s.U_INFORMANTE).WithMany().HasPrincipalKey(u => u.CODIGO).HasForeignKey(s => s.UNIDAD_INFORMANTE);
            // Crea variable navegacion U_AFECTADA para relacionar campo UNIDAD_AFECTADA de entidad incidentes con CODIGO de la entidad unidad
            modelBuilder.Entity<incidentes>().HasOne(s => s.U_AFECTADA).WithMany().HasPrincipalKey(u => u.CODIGO).HasForeignKey(s => s.UNIDAD_AFECTADA);

            //Incidentes
            modelBuilder.ApplyConfiguration(new PreliminaresMap());
            modelBuilder.ApplyConfiguration(new InvolucradosPreMap());
            modelBuilder.ApplyConfiguration(new AntecedentesJudicialesPreMap());
            modelBuilder.ApplyConfiguration(new ProcedimientosPolicialesPreMap());
            modelBuilder.ApplyConfiguration(new ArchivosInformesPreMap());

            modelBuilder.ApplyConfiguration(new IncidentesMap());
            modelBuilder.ApplyConfiguration(new InvolucradosIncMap());
            modelBuilder.ApplyConfiguration(new AntecedentesJudicialesIncMap());
            modelBuilder.ApplyConfiguration(new ProcedimientosPolicialesIncMap());
            modelBuilder.ApplyConfiguration(new ArchivosInformesIncMap());
            modelBuilder.ApplyConfiguration(new AmpliacionesInformesIncMap());
            modelBuilder.ApplyConfiguration(new MotivoDevolucionIncMap());

            modelBuilder.ApplyConfiguration(new FinalesMap());
            modelBuilder.ApplyConfiguration(new InvolucradosFinMap());
            modelBuilder.ApplyConfiguration(new AntecedentesJudicialesFinMap());
            modelBuilder.ApplyConfiguration(new ProcedimientosPolicialesFinMap());
            modelBuilder.ApplyConfiguration(new ArchivosInformesFinMap());
            modelBuilder.ApplyConfiguration(new AmpliacionesInformesFinMap());
            modelBuilder.ApplyConfiguration(new MedidasAdoptadasFinMap());
            modelBuilder.ApplyConfiguration(new MotivoDevolucionFinMap());

            modelBuilder.ApplyConfiguration(new EstadoSaludMap());
            modelBuilder.ApplyConfiguration(new PersonasCivilesMap());
            modelBuilder.ApplyConfiguration(new TipoArmamentoMap());
            modelBuilder.ApplyConfiguration(new TipoConsumoMap());
            modelBuilder.ApplyConfiguration(new TipoIdentificacionMap());
            modelBuilder.ApplyConfiguration(new TipoGrupoIncidenteMap());
            modelBuilder.ApplyConfiguration(new TipoIncidenteMap());
            modelBuilder.ApplyConfiguration(new TipoIntervinienteMap());
            modelBuilder.ApplyConfiguration(new TipoInvestigacionMap());
            modelBuilder.ApplyConfiguration(new TipoIngestaMap());
            modelBuilder.ApplyConfiguration(new PersonaSciMap());
            modelBuilder.ApplyConfiguration(new CondicionPolicialMap());
            modelBuilder.ApplyConfiguration(new IndicesAlertasMap());
            modelBuilder.ApplyConfiguration(new MedidasAdoptadasMap());

            modelBuilder.ApplyConfiguration(new RegistroEventosMap());
        }
    }
}
