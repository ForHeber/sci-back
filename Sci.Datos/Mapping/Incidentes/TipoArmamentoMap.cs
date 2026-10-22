using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sci.Entidades;

namespace Sci.Datos.Mapping
{
    class TipoArmamentoMap : IEntityTypeConfiguration<tipo_armamento>
    {
        public void Configure(EntityTypeBuilder<tipo_armamento> builder)
        {
            builder.ToTable("TIPO_ARMAMENTO").HasKey(a => a.ID);
        }
    }
}
