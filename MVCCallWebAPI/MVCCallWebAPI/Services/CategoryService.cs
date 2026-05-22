using MVCCallWebAPI.DTOs;
using MVCCallWebAPI.Services.Interface;
using MVCCallWebAPI.ViewModels;

namespace MVCCallWebAPI.Services;

public class CategoryService : ICategoryService
{
    private readonly IApiService _apiService;

    public CategoryService(
        IApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<IEnumerable<CategoryViewModel>> GetCategoriesAsync()
    {
        var categories =
            await _apiService.GetAsync<IEnumerable<CategoryDto>>("api/categories");

        if (categories == null)
            return new List<CategoryViewModel>();

        return categories.Select(c => new CategoryViewModel
        {
            Id = c.Id,
            Name = c.Name
        });
    }
    public async Task<CategoryDto> CreateAsync(CategoryDto dto)
    {
        var createdCategory = await _apiService.PostAsync<CategoryDto>("api/categories", dto);
        return createdCategory;
    }

}
