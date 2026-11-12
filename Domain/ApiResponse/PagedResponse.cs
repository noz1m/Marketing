using System.Net;

namespace Domain.ApiResponse;

public sealed class PagedResponse<T>(
    List<T> items,
    int totalRecords,
    int pageNumber,
    int pageSize)
    : Response<List<T>>(true, items, "Items retrieved successfully.", HttpStatusCode.OK)
{
    public int PageNumber { get; } = pageNumber;
    public int PageSize { get; } = pageSize;
    public int TotalRecords { get; } = totalRecords;
    public int TotalPages { get; } = (int)Math.Ceiling((double)totalRecords / pageSize);
}
