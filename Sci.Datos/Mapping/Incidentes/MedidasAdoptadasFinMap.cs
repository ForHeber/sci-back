using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sci.Entidades;

namespace Sci.Datos.Mapping
{
    class MedidasAdoptadasFinMap : IEntityTypeConfiguration<medidas_adoptadas_fin>
    {
        public void Configure(EntityTypeBuilder<medidas_adoptadas_fin> builder)
        {
            builder.ToTable("MEDIDAS_ADOPTADAS_FIN").HasKey(a => a.ID);
        }
    }
}
