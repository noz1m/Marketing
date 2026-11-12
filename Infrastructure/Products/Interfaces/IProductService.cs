using Domain.ApiResponse;
using Domain.DTOs.Products;
using Domain.Filters;

namespace Infrastructure.Products.Interfaces;

public interface IProductService
{
    Task<PagedResponse<GetProductDTO>> GetPagedAsync(ProductFilter filter, CancellationToken cancellationToken = default);
    Task<Response<GetProductDTO>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Response<GetProductDTO>> CreateAsync(CreateProductDTO dto, CancellationToken cancellationToken = default);
    Task<Response<GetProductDTO>> UpdateAsync(int id, UpdateProductDTO dto, CancellationToken cancellationToken = default);
    Task<Response<string>> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
