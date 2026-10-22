using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sci.Entidades;

namespace Sci.Datos.Mapping
{
    class PersonaMap : IEntityTypeConfiguration<persona>
    {
        public void Configure(EntityTypeBuilder<persona> builder)
        {
            builder.ToTable("PERSONA");
        }
    }
}
