using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sci.Entidades;

namespace Sci.Datos.Mapping
{
    class PreliminaresMap : IEntityTypeConfiguration<preliminares>
    {
        public void Configure(EntityTypeBuilder<preliminares> builder)
        {
            builder.ToTable("PRELIMINARES").HasKey(a => a.ID);
        }
    }
}
