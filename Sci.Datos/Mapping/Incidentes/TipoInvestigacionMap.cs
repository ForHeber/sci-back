using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sci.Entidades;

namespace Sci.Datos.Mapping
{
    class TipoInvestigacionMap : IEntityTypeConfiguration<tipo_investigacion>
    {
        public void Configure(EntityTypeBuilder<tipo_investigacion> builder)
        {
            builder.ToTable("TIPO_INVESTIGACION").HasKey(a => a.ID);
        }
    }
}
