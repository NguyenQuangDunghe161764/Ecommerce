using AutoMapper;
using demoWebAPI.Models;
using Microsoft.EntityFrameworkCore;

public class CategoryService : ICategoryService
{
    private readonly EcomDbContext _repository;
    private readonly IMapper _mapper;

    public CategoryService(EcomDbContext repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<List<CategoryDto>> GetAllAsync()
    {
        var categories = await _repository.Categories.ToListAsync();
        return _mapper.Map<List<CategoryDto>>(categories);
    }

    public async Task<CategoryDto> CreateAsync(CategoryDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new Exception("Name is required");

        var entity = new Category
        {
            Name = dto.Name
        };

        await _repository.Categories.AddAsync(entity);
        await _repository.SaveChangesAsync();

        return new CategoryDto
        {
            Id = entity.Id,
            Name = entity.Name
        };
    }
}
