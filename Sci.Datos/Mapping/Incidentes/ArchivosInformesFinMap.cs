using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sci.Entidades;

namespace Sci.Datos.Mapping
{
    class ArchivosInformesFinMap : IEntityTypeConfiguration<archivos_informes_fin>
    {
        public void Configure(EntityTypeBuilder<archivos_informes_fin> builder)
        {
            builder.ToTable("ARCHIVOS_INFORMES_FIN").HasKey(a => a.ID);
        }
    }
}
