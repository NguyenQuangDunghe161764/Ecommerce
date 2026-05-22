using MVCCallWebAPI.DTOs;
using MVCCallWebAPI.ViewModels;

public interface ICategoryService
{
    Task<IEnumerable<CategoryViewModel>> GetCategoriesAsync();
    Task<CategoryDto> CreateAsync(CategoryDto dto);
}