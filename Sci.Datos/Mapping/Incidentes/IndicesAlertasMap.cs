using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sci.Entidades;

namespace Sci.Datos.Mapping
{
    class IndicesAlertasMap : IEntityTypeConfiguration<indices_alertas>
    {
        public void Configure(EntityTypeBuilder<indices_alertas> builder)
        {
            builder.ToTable("INDICES_ALERTAS").HasKey(a => a.ID);
        }
    }
}
