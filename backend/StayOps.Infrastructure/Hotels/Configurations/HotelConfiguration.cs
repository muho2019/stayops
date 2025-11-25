using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StayOps.Domain.Hotels;

namespace StayOps.Infrastructure.Hotels.Configurations;

public sealed class HotelConfiguration : IEntityTypeConfiguration<Hotel>
{
    public void Configure(EntityTypeBuilder<Hotel> builder)
    {
        builder.ToTable("Hotels", "inventory");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.Id)
            .HasConversion(
                id => id.Value,
                value => new HotelId(value));

        builder.Property(h => h.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(h => h.Code)
            .IsUnique();

        builder.Property(h => h.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(h => h.Timezone)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(h => h.Status)
            .HasConversion(
                status => status.ToString(),
                value => Enum.Parse<HotelStatus>(value, true))
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(h => h.IsDeleted)
            .HasDefaultValue(false);

        builder.Property(h => h.CreatedAt)
            .IsRequired();

        builder.Property(h => h.UpdatedAt)
            .IsRequired();
    }
}
