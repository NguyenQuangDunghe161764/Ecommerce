using AutoMapper;
using demoWebAPI.Models;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Product, ProductDto>()
            .ForMember(
                dest => dest.MainImageUrl,
                opt => opt.MapFrom(src =>
                    src.Productimages
                        .Where(x => x.IsMain)
                        .Select(x => x.ImageUrl)
                        .FirstOrDefault()
                )
            );

        CreateMap<Category, CategoryDto>();
    }
}