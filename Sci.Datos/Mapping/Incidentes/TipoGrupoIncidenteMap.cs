using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sci.Entidades;

namespace Sci.Datos.Mapping
{
    class TipoGrupoIncidenteMap : IEntityTypeConfiguration<tipo_grupo_incidente>
    {
        public void Configure(EntityTypeBuilder<tipo_grupo_incidente> builder)
        {
            builder.ToTable("TIPO_GRUPO_INCIDENTE").HasKey(a => a.ID);
        }
    }
}
