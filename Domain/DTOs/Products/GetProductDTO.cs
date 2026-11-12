namespace Domain.DTOs.Products;

public sealed record GetProductDTO(int Id, string Name, decimal Price, DateTimeOffset CreatedAt);
