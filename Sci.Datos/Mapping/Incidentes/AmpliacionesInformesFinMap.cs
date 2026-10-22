using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sci.Entidades;

namespace Sci.Datos.Mapping
{
    class AmpliacionesInformesFinMap : IEntityTypeConfiguration<ampliaciones_informes_fin>
    {
        public void Configure(EntityTypeBuilder<ampliaciones_informes_fin> builder)
        {
            builder.ToTable("AMPLIACIONES_INFORMES_FIN").HasKey(a => a.ID);
        }
    }
}
