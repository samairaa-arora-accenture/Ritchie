using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Richie.Domain.Insurance;

namespace Richie.Infrastructure.Persistence.Configurations;

public sealed class InsurancePolicyConfiguration : IEntityTypeConfiguration<InsurancePolicy>
{
    public void Configure(EntityTypeBuilder<InsurancePolicy> builder)
    {
        builder.HasKey(p => p.Id);
        builder.HasIndex(p => new { p.UserId, p.RenewalDate });
        builder.Property(p => p.PolicyName).HasMaxLength(200);
        builder.Property(p => p.PolicyNumber).HasMaxLength(100);
        builder.Property(p => p.Provider).HasMaxLength(150);
        builder.Property(p => p.Nominee).HasMaxLength(150);
        builder.Property(p => p.Notes).HasMaxLength(2000);
    }
}
