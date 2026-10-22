using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sci.Entidades;

namespace Sci.Datos.Mapping
{
    class TipoIngestaMap : IEntityTypeConfiguration<tipo_ingesta>
    {
        public void Configure(EntityTypeBuilder<tipo_ingesta> builder)
        {
            builder.ToTable("TIPO_INGESTA").HasKey(a => a.ID);
        }
    }
}
