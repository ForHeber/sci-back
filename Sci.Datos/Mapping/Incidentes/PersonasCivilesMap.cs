using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sci.Entidades;

namespace Sci.Datos.Mapping
{
    class PersonasCivilesMap : IEntityTypeConfiguration<personas_civiles>
    {
        public void Configure(EntityTypeBuilder<personas_civiles> builder)
        {
            builder.ToTable("PERSONAS_CIVILES").HasKey(a => a.RUN);
        }
    }
}
