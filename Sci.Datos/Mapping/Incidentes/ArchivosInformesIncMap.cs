using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sci.Entidades;

namespace Sci.Datos.Mapping
{
    class ArchivosInformesIncMap : IEntityTypeConfiguration<archivos_informes_inc>
    {
        public void Configure(EntityTypeBuilder<archivos_informes_inc> builder)
        {
            builder.ToTable("ARCHIVOS_INFORMES_INC").HasKey(a => a.ID);
        }
    }
}
