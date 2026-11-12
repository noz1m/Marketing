using Domain.Entities;
using Domain.Filters;
using Infrastructure.Data;
using Infrastructure.Products.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Products.Repositories;

public sealed class ProductRepository(DataContext context) : IProductRepository
{
    public Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => context.Products.FirstOrDefaultAsync(product => product.Id == id, cancellationToken);

    public async Task<(List<Product> Items, int TotalCount)> GetPagedAsync(
        ProductFilter filter, CancellationToken cancellationToken = default)
    {
        IQueryable<Product> query = context.Products.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filter.Name))
        {
            var term = filter.Name.Trim()
                .Replace("\\", "\\\\")
                .Replace("%", "\\%")
                .Replace("_", "\\_");
            query = query.Where(product => EF.Functions.ILike(product.Name, $"%{term}%", "\\"));
        }

        if (filter.StartPrice is { } startPrice)
            query = query.Where(product => product.Price >= startPrice);

        if (filter.ToPrice is { } endPrice)
            query = query.Where(product => product.Price <= endPrice);

        var totalCount = await query.CountAsync(cancellationToken);
        var offset = (long)(filter.PageNumber - 1) * filter.PageSize;
        if (offset >= totalCount)
            return ([], totalCount);

        var items = await query.OrderBy(product => product.Id)
            .Skip((int)offset)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<Product> CreateAsync(Product product, CancellationToken cancellationToken = default)
    {
        await context.Products.AddAsync(product, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return product;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => context.SaveChangesAsync(cancellationToken);

    public async Task DeleteAsync(Product product, CancellationToken cancellationToken = default)
    {
        context.Products.Remove(product);
        await context.SaveChangesAsync(cancellationToken);
    }
}
