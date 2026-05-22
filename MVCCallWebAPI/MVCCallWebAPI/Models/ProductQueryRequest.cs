using Microsoft.AspNetCore.Mvc;

namespace MVCCallWebAPI.Models
{
    public class ProductQueryRequest
    {
        public string? Keyword { get; set; }
        public int? CategoryId { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
