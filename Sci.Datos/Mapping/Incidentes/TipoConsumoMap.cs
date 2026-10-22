using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sci.Entidades;

namespace Sci.Datos.Mapping
{
    class TipoConsumoMap : IEntityTypeConfiguration<tipo_consumo>
    {
        public void Configure(EntityTypeBuilder<tipo_consumo> builder)
        {
            builder.ToTable("TIPO_CONSUMO").HasKey(a => a.ID);
        }
    }
}
