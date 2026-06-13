using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Richie.Domain.Authentication;

namespace Richie.Infrastructure.Persistence.Configurations;

public sealed class SecurityAnswerConfiguration : IEntityTypeConfiguration<SecurityAnswer>
{
    public void Configure(EntityTypeBuilder<SecurityAnswer> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Question).IsRequired();
        builder.Property(a => a.AnswerHash).IsRequired();
    }
}
