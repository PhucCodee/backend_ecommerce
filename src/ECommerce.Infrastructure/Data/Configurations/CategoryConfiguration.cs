using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ECommerce.Domain.Entities;

namespace ECommerce.Infrastructure.Data.Configurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.ToTable("categories");

            builder.HasKey(c => c.CategoryId);

            builder.Property(c => c.CategoryId)
                .HasColumnName("category_id");

            builder.Property(c => c.CategoryName)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("category_name");

            builder.Property(c => c.Slug)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("slug");

            builder.Property(c => c.ParentCategoryId)
                .HasColumnName("parent_category_id");

            builder.Property(c => c.Description)
                .HasMaxLength(500)
                .HasColumnName("description");

            builder.Property(c => c.ImageUrl)
                .HasMaxLength(255)
                .HasColumnName("image_url");

            builder.Property(c => c.DisplayOrder)
                .HasColumnName("display_order");

            builder.Property(c => c.IsCore)
                .HasColumnName("is_core")
                .HasDefaultValue(true);

            builder.Property(c => c.IsActive)
                .HasColumnName("is_active")
                .HasDefaultValue(true);

            builder.Property(c => c.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(c => c.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.HasOne(c => c.ParentCategory)
                .WithMany(c => c.ChildCategories)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}