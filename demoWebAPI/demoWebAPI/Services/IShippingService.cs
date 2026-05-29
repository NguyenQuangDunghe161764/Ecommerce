using demoWebAPI.Models; 
namespace demoWebAPI.Services
{
    public interface IShippingService
    {
        Task<bool> SaveAddressAsync(AddressDto model);
        Task<decimal> CalculateShippingFeeAsync(string province);
    }
}