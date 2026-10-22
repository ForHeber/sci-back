using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sci.Entidades;

namespace Sci.Datos.Mapping
{
    class ArchivosInformesPreMap : IEntityTypeConfiguration<archivos_informes_pre>
    {
        public void Configure(EntityTypeBuilder<archivos_informes_pre> builder)
        {
            builder.ToTable("ARCHIVOS_INFORMES_PRE").HasKey(a => a.ID);
        }
    }
}
