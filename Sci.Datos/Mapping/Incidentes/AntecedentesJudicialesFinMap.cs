using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sci.Entidades;

namespace Sci.Datos.Mapping
{
    class AntecedentesJudicialesFinMap : IEntityTypeConfiguration<antecedentes_judiciales_fin>
    {
        public void Configure(EntityTypeBuilder<antecedentes_judiciales_fin> builder)
        {
            builder.ToTable("ANTECEDENTES_JUDICIALES_FIN").HasKey(a => a.ID);
        }
    }
}
