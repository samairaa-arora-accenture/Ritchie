using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Richie.Domain.Assets;

namespace Richie.Infrastructure.Persistence.Configurations;

public sealed class SipContributionConfiguration : IEntityTypeConfiguration<SipContribution>
{
    public void Configure(EntityTypeBuilder<SipContribution> builder)
    {
        builder.HasKey(c => c.Id);
        builder.HasIndex(c => c.AssetId);

        builder.HasOne<Asset>()
            .WithMany()
            .HasForeignKey(c => c.AssetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
