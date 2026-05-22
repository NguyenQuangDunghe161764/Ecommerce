using MVCCallWebAPI.Models;
using MVCCallWebAPI.ViewModels;
namespace MVCCallWebAPI.Services.Interface;

public interface IProductService
{
    Task<ProductViewModel?> GetProductByIdAsync(int id);
    Task<PagedResult<ProductViewModel>> GetProductsAsync(
    string? keyword,
    int? categoryId,
    int page,
    int pageSize);
}