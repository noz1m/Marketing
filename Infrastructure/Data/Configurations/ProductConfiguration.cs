using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(product => product.Id);
        builder.Property(product => product.Name).HasColumnType("text").IsRequired();
        builder.Property(product => product.Price).HasColumnType("numeric").IsRequired();
        builder.Property(product => product.CreatedAt)
            .HasColumnType("timestamp with time zone").IsRequired();
    }
}
