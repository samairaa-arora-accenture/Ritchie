using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Richie.Domain.Notifications;

namespace Richie.Infrastructure.Persistence.Configurations;

public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(n => n.Id);
        builder.HasIndex(n => new { n.UserId, n.IsRead });
        builder.Property(n => n.Title).IsRequired().HasMaxLength(120);
        builder.Property(n => n.Message).HasMaxLength(500);
    }
}
