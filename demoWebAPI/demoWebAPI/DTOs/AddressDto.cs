
namespace demoWebAPI.Models
{
    public class AddressDto
    {
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string Ward { get; set; } = string.Empty;
        public string DetailAddress { get; set; } = string.Empty;

        public string FullAddress { get; set; } = string.Empty;

        public bool IsDefault { get; set; }
    }
}