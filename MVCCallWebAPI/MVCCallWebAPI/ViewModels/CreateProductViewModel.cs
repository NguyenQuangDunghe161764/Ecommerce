using Microsoft.AspNetCore.Mvc;

namespace MVCCallWebAPI.ViewModels
{
    public class CreateProductViewModel
    {
        public string Name { get; set; }
        public List<IFormFile>? Images
        {
            get; set;
        }
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public int Stock { get; set; }
        public string Description { get; set; }
    }
}
