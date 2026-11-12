using System.Net;
using Domain.ApiResponse;
using Domain.DTOs.Products;
using Domain.Filters;
using Infrastructure.Products.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Manager")]
[Route("api/[controller]")]
public sealed class ProductController(IProductService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResponse<GetProductDTO>>> GetProductsAsync(
        [FromQuery] ProductFilter filter, CancellationToken cancellationToken)
    {
        if (filter.StartPrice < 0 || filter.ToPrice < 0 || filter.StartPrice > filter.ToPrice)
            return BadRequest(Response<string>.Fail("Invalid price range.", HttpStatusCode.BadRequest));

        return Ok(await service.GetPagedAsync(filter, cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Response<GetProductDTO>>> GetProductAsync(
        int id, CancellationToken cancellationToken)
    {
        var result = await service.GetByIdAsync(id, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost]
    public async Task<ActionResult<Response<GetProductDTO>>> AddProductAsync(
        [FromBody] CreateProductDTO dto, CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Response<GetProductDTO>>> UpdateProductAsync(
        int id, [FromBody] UpdateProductDTO dto, CancellationToken cancellationToken)
    {
        var result = await service.UpdateAsync(id, dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<Response<string>>> DeleteProductAsync(
        int id, CancellationToken cancellationToken)
    {
        var result = await service.DeleteAsync(id, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
