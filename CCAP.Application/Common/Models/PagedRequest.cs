namespace CCAP.Application.Common.Models;

public class PagedRequest
{
    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public string? Search { get; set; }

    public void Normalize()
    {
        if (PageNumber < 1)
            PageNumber = 1;

        if (PageSize < 1)
            PageSize = 20;

        if (PageSize > 100)
            PageSize = 100;

        Search = string.IsNullOrWhiteSpace(Search)
            ? null
            : Search.Trim();
    }
}