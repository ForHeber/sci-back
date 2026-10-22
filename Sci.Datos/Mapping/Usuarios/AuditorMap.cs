using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sci.Entidades;

namespace Sci.Datos.Mapping
{
    class AuditorMap : IEntityTypeConfiguration<auditor>
    {
        public void Configure(EntityTypeBuilder<auditor> builder)
        {
            builder.ToTable("AUDITOR").HasKey(a => a.ID);
        }
    }
}
