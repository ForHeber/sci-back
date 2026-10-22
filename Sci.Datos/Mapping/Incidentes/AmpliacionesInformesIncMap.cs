using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sci.Entidades;

namespace Sci.Datos.Mapping
{
    class AmpliacionesInformesIncMap : IEntityTypeConfiguration<ampliaciones_informes_inc>
    {
        public void Configure(EntityTypeBuilder<ampliaciones_informes_inc> builder)
        {
            builder.ToTable("AMPLIACIONES_INFORMES_INC").HasKey(a => a.ID);
        }
    }
}
