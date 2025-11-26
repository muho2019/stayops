using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StayOps.Domain.Hotels;
using StayOps.Domain.Rooms;
using StayOps.Domain.RoomTypes;

namespace StayOps.Infrastructure.Rooms.Configurations;

public sealed class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.ToTable("Rooms", "inventory");

        builder.HasQueryFilter(r => !r.IsDeleted);

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasConversion(
                id => id.Value,
                value => new RoomId(value));

        builder.Property(r => r.HotelId)
            .HasConversion(
                id => id.Value,
                value => new HotelId(value))
            .IsRequired();

        builder.Property(r => r.RoomTypeId)
            .HasConversion(
                id => id.Value,
                value => new RoomTypeId(value))
            .IsRequired();

        builder.Property(r => r.Number)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(r => r.Floor)
            .IsRequired(false);

        builder.Property(r => r.Status)
            .HasConversion(
                status => status.ToString(),
                value => Enum.Parse<RoomStatus>(value, true))
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(r => r.HousekeepingStatus)
            .HasConversion(
                status => status.ToString(),
                value => Enum.Parse<HousekeepingStatus>(value, true))
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(r => r.IsDeleted)
            .HasDefaultValue(false);

        builder.Property(r => r.CreatedAt)
            .IsRequired();

        builder.Property(r => r.UpdatedAt)
            .IsRequired();

        builder.Property(r => r.RowVersion)
            .IsRowVersion();

        // 컬럼명을 메타데이터에서 얻어 안전한 필터 문자열 구성
        var isDeletedProperty = builder.Metadata.FindProperty(nameof(Hotel.IsDeleted));
        var storeObject = StoreObjectIdentifier.Table(builder.Metadata.GetTableName() ?? "Rooms", builder.Metadata.GetSchema());
        var isDeletedColumnName = isDeletedProperty?.GetColumnName(storeObject) ?? nameof(Hotel.IsDeleted);
        var filter = (isDeletedProperty?.IsNullable ?? false)
            ? $"[{isDeletedColumnName}] = 0 OR [{isDeletedColumnName}] IS NULL"
            : $"[{isDeletedColumnName}] = 0";
        builder.HasIndex(r => new { r.HotelId, r.Number })
            .IsUnique()
            .HasFilter(filter);
    }
}
