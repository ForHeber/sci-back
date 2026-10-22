using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sci.Entidades;

namespace Sci.Datos.Mapping
{
    class FinalesMap : IEntityTypeConfiguration<finales>
    {
        public void Configure(EntityTypeBuilder<finales> builder)
        {
            builder.ToTable("FINALES").HasKey(a => a.ID);
        }
    }
}
