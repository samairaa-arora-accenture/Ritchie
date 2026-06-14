using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Richie.Domain.Settings;

namespace Richie.Infrastructure.Persistence.Configurations;

public sealed class AppSettingsConfiguration : IEntityTypeConfiguration<AppSettings>
{
    public void Configure(EntityTypeBuilder<AppSettings> builder)
    {
        builder.HasKey(s => s.UserId);
        builder.Property(s => s.Theme).HasMaxLength(20);
        builder.Property(s => s.BackupFrequency).HasMaxLength(20);
        builder.Property(s => s.DisabledNotificationTypes).HasMaxLength(300);
    }
}
