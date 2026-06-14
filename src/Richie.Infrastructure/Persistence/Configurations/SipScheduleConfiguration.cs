using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Richie.Domain.Assets;

namespace Richie.Infrastructure.Persistence.Configurations;

public sealed class SipScheduleConfiguration : IEntityTypeConfiguration<SipSchedule>
{
    public void Configure(EntityTypeBuilder<SipSchedule> builder)
    {
        builder.HasKey(s => s.Id);
        builder.HasIndex(s => s.AssetId).IsUnique();   // one schedule per asset
        builder.HasIndex(s => new { s.IsEnabled, s.NextRunDateUtc });

        builder.HasOne<Asset>()
            .WithMany()
            .HasForeignKey(s => s.AssetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
