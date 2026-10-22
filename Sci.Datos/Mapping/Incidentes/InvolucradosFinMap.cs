using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sci.Entidades;

namespace Sci.Datos.Mapping
{
    class InvolucradosFinMap : IEntityTypeConfiguration<involucrados_fin>
    {
        public void Configure(EntityTypeBuilder<involucrados_fin> builder)
        {
            builder.ToTable("INVOLUCRADOS_FIN").HasKey(a => a.ID);
        }
    }
}
