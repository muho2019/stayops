using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StayOps.Domain.Hotels;
using StayOps.Domain.RoomTypes;

namespace StayOps.Infrastructure.RoomTypes.Configurations;

public sealed class RoomTypeConfiguration : IEntityTypeConfiguration<RoomType>
{
    public void Configure(EntityTypeBuilder<RoomType> builder)
    {
        builder.ToTable("RoomTypes", "inventory");

        builder.HasQueryFilter(rt => !rt.IsDeleted);

        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.Id)
            .HasConversion(
                id => id.Value,
                value => new RoomTypeId(value));

        builder.Property(rt => rt.HotelId)
            .HasConversion(
                id => id.Value,
                value => new HotelId(value))
            .IsRequired();

        builder.Property(rt => rt.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(rt => rt.Description)
            .HasMaxLength(1000);

        builder.Property(rt => rt.Capacity)
            .IsRequired();

        builder.Property(rt => rt.BaseRate)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(rt => rt.Status)
            .HasConversion(
                status => status.ToString(),
                value => Enum.Parse<RoomTypeStatus>(value, true))
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(rt => rt.IsDeleted)
            .HasDefaultValue(false);

        builder.Property(rt => rt.CreatedAt)
            .IsRequired();

        builder.Property(rt => rt.UpdatedAt)
            .IsRequired();

        // 컬럼명을 메타데이터에서 얻어 안전한 필터 문자열 구성
        var isDeletedProperty = builder.Metadata.FindProperty(nameof(Hotel.IsDeleted));
        var storeObject = StoreObjectIdentifier.Table(builder.Metadata.GetTableName() ?? "RoomTypes", builder.Metadata.GetSchema());
        var isDeletedColumnName = isDeletedProperty?.GetColumnName(storeObject) ?? nameof(Hotel.IsDeleted);
        var filter = (isDeletedProperty?.IsNullable ?? false)
            ? $"[{isDeletedColumnName}] = 0 OR [{isDeletedColumnName}] IS NULL"
            : $"[{isDeletedColumnName}] = 0";
        builder.HasIndex(rt => new { rt.HotelId, rt.Name })
            .IsUnique()
            .HasFilter(filter);
    }
}
