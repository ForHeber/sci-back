using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sci.Entidades;

namespace Sci.Datos.Mapping
{
    class AntecedentesJudicialesIncMap : IEntityTypeConfiguration<antecedentes_judiciales_inc>
    {
        public void Configure(EntityTypeBuilder<antecedentes_judiciales_inc> builder)
        {
            builder.ToTable("ANTECEDENTES_JUDICIALES_INC").HasKey(a => a.ID);
        }
    }
}
