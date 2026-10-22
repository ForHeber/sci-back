using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sci.Entidades;

namespace Sci.Datos.Mapping
{
    class AntecedentesJudicialesPreMap : IEntityTypeConfiguration<antecedentes_judiciales_pre>
    {
        public void Configure(EntityTypeBuilder<antecedentes_judiciales_pre> builder)
        {
            builder.ToTable("ANTECEDENTES_JUDICIALES_PRE").HasKey(a => a.ID);
        }
    }
}
