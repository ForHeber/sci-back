using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sci.Entidades;

namespace Sci.Datos.Mapping
{
    class MedidasAdoptadasMap : IEntityTypeConfiguration<medidas_adoptadas>
    {
        public void Configure(EntityTypeBuilder<medidas_adoptadas> builder)
        {
            builder.ToTable("MEDIDAS_ADOPTADAS").HasKey(a => a.ID);
        }
    }
}
