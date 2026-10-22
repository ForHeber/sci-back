using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sci.Entidades;

namespace Sci.Datos.Mapping
{
    class TipoIdentificacionMap : IEntityTypeConfiguration<tipo_identificacion>
    {
        public void Configure(EntityTypeBuilder<tipo_identificacion> builder)
        {
            builder.ToTable("TIPO_IDENTIFICACION").HasKey(a => a.ID);
        }
    }
}
