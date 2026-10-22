using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sci.Entidades;

namespace Sci.Datos.Mapping
{
    class RegistroEventosMap : IEntityTypeConfiguration<registro_eventos>
    {
        public void Configure(EntityTypeBuilder<registro_eventos> builder)
        {
            builder.ToTable("REGISTRO_EVENTOS").HasKey(a => a.ID);
        }
    }
}
