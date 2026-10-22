using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sci.Entidades;

namespace Sci.Datos.Mapping
{
    class PersonaSciMap : IEntityTypeConfiguration<persona_sci>
    {
        public void Configure(EntityTypeBuilder<persona_sci> builder)
        {
            builder.ToTable("PERSONA_SCI");
        }
    }
}
