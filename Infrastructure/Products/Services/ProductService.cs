using System.Net;
using Domain.ApiResponse;
using Domain.DTOs.Products;
using Domain.Entities;
using Domain.Filters;
using Infrastructure.Products.Interfaces;
using Infrastructure.RedisCache.Interfaces;

namespace Infrastructure.Products.Services;

public sealed class ProductService(IProductRepository repository, ICacheService cache) : IProductService
{
    private static string CacheKey(int id) => $"product:{id}";

    private static GetProductDTO Map(Product product)
        => new(product.Id, product.Name, product.Price, product.CreatedAt);

    public async Task<PagedResponse<GetProductDTO>> GetPagedAsync(
        ProductFilter filter, CancellationToken cancellationToken = default)
    {
        var (items, count) = await repository.GetPagedAsync(filter, cancellationToken);
        return new PagedResponse<GetProductDTO>(items.Select(Map).ToList(),
            count, filter.PageNumber, filter.PageSize);
    }

    public async Task<Response<GetProductDTO>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var cached = await cache.GetAsync<GetProductDTO>(CacheKey(id), cancellationToken);
        if (cached is not null)
            return Response<GetProductDTO>.Success(cached);

        var product = await repository.GetByIdAsync(id, cancellationToken);
        if (product is null)
            return Response<GetProductDTO>.Fail("Product not found.", HttpStatusCode.NotFound);

        var dto = Map(product);
        await cache.SetAsync(CacheKey(id), dto, TimeSpan.FromMinutes(5), cancellationToken);
        return Response<GetProductDTO>.Success(dto);
    }

    public async Task<Response<GetProductDTO>> CreateAsync(
        CreateProductDTO dto, CancellationToken cancellationToken = default)
    {
        var product = new Product
        {
            Name = dto.Name.Trim(),
            Price = dto.Price,
            CreatedAt = DateTimeOffset.UtcNow
        };

        if (string.IsNullOrWhiteSpace(product.Name))
            return Response<GetProductDTO>.Fail("Product name is required.", HttpStatusCode.BadRequest);

        await repository.CreateAsync(product, cancellationToken);
        return Response<GetProductDTO>.Created(Map(product), "Product created successfully.");
    }

    public async Task<Response<GetProductDTO>> UpdateAsync(
        int id, UpdateProductDTO dto, CancellationToken cancellationToken = default)
    {
        var product = await repository.GetByIdAsync(id, cancellationToken);
        if (product is null)
            return Response<GetProductDTO>.Fail("Product not found.", HttpStatusCode.NotFound);

        if (string.IsNullOrWhiteSpace(dto.Name))
            return Response<GetProductDTO>.Fail("Product name is required.", HttpStatusCode.BadRequest);

        product.Name = dto.Name.Trim();
        product.Price = dto.Price;
        await repository.SaveChangesAsync(cancellationToken);
        await cache.RemoveAsync(CacheKey(id), cancellationToken);
        return Response<GetProductDTO>.Success(Map(product), "Product updated successfully.");
    }

    public async Task<Response<string>> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await repository.GetByIdAsync(id, cancellationToken);
        if (product is null)
            return Response<string>.Fail("Product not found.", HttpStatusCode.NotFound);

        await repository.DeleteAsync(product, cancellationToken);
        await cache.RemoveAsync(CacheKey(id), cancellationToken);
        return Response<string>.Success("Deleted", "Product deleted successfully.");
    }
}
