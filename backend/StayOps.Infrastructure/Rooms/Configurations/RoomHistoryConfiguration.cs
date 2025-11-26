using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StayOps.Domain.Hotels;
using StayOps.Domain.Rooms;
using StayOps.Domain.RoomTypes;

namespace StayOps.Infrastructure.Rooms.Configurations;

public sealed class RoomHistoryConfiguration : IEntityTypeConfiguration<RoomHistoryEntry>
{
    public void Configure(EntityTypeBuilder<RoomHistoryEntry> builder)
    {
        builder.ToTable("RoomHistory", "inventory");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.RoomId)
            .HasConversion(
                id => id.Value,
                value => new RoomId(value))
            .IsRequired();

        builder.Property(h => h.HotelId)
            .HasConversion(
                id => id.Value,
                value => new HotelId(value))
            .IsRequired();

        builder.Property(h => h.RoomTypeId)
            .HasConversion(
                id => id.Value,
                value => new RoomTypeId(value))
            .IsRequired();

        builder.Property(h => h.Status)
            .HasConversion(
                status => status.HasValue ? status.Value.ToString() : null,
                value => string.IsNullOrWhiteSpace(value) ? null : Enum.Parse<RoomStatus>(value, true))
            .HasMaxLength(32);

        builder.Property(h => h.HousekeepingStatus)
            .HasConversion(
                status => status.HasValue ? status.Value.ToString() : null,
                value => string.IsNullOrWhiteSpace(value) ? null : Enum.Parse<HousekeepingStatus>(value, true))
            .HasMaxLength(32);

        builder.Property(h => h.Action)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(h => h.Reason)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(h => h.CreatedAt)
            .IsRequired();

        builder.HasIndex(h => new { h.RoomId, h.CreatedAt });
    }
}
