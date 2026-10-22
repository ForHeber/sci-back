using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sci.Entidades;

namespace Sci.Datos.Mapping
{
    class TipoIntervinienteMap : IEntityTypeConfiguration<tipo_interviniente>
    {
        public void Configure(EntityTypeBuilder<tipo_interviniente> builder)
        {
            builder.ToTable("TIPO_INTERVINIENTE").HasKey(a => a.ID);
        }
    }
}
