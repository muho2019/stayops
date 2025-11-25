using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StayOps.Domain.Hotels;

namespace StayOps.Infrastructure.Hotels.Configurations;

public sealed class HotelConfiguration : IEntityTypeConfiguration<Hotel>
{
    public void Configure(EntityTypeBuilder<Hotel> builder)
    {
        builder.ToTable("Hotels", "inventory");

        builder.HasQueryFilter(h => !h.IsDeleted);

        builder.HasKey(h => h.Id);

        builder.Property(h => h.Id)
            .HasConversion(
                id => id.Value,
                value => new HotelId(value));

        builder.Property(h => h.Code)
            .HasMaxLength(50)
            .IsRequired();

        // 컬럼명을 메타데이터에서 얻어 안전한 필터 문자열 구성
        var isDeletedProperty = builder.Metadata.FindProperty(nameof(Hotel.IsDeleted));
        var storeObject = StoreObjectIdentifier.Table(builder.Metadata.GetTableName() ?? "Hotels", builder.Metadata.GetSchema());
        var isDeletedColumnName = isDeletedProperty?.GetColumnName(storeObject) ?? nameof(Hotel.IsDeleted);
        var codeFilter = (isDeletedProperty?.IsNullable ?? false)
            ? $"[{isDeletedColumnName}] = 0 OR [{isDeletedColumnName}] IS NULL"
            : $"[{isDeletedColumnName}] = 0";
        builder.HasIndex(h => h.Code)
            .IsUnique()
            .HasFilter(codeFilter);

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
