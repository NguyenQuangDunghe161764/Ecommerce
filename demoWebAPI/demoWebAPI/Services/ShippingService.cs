using demoWebAPI.Models;
using demoWebAPI.Data;

namespace demoWebAPI.Services
{
    public class ShippingService : IShippingService
    {
        private readonly EcomDbContext _context;
        // Inject DbContext vào Service
        public ShippingService(EcomDbContext context)
        {
            _context = context;
        }

        public async Task<bool> SaveAddressAsync(AddressDto model)
        {
            try
            {
                // Logic map từ DTO sang Entity và lưu vào cơ sở dữ liệu
                var address = new Address
                {
                    FullName = model.FullName,
                    PhoneNumber = model.PhoneNumber,
                    Province = model.Province,
                    District = model.District,
                    Ward = model.Ward,
                    DetailAddress = model.DetailAddress,
                    IsDefault = model.IsDefault
                };

                _context.UserAddresses.Add(address);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Log lỗi nếu có
                return false;
            }
        }

        public async Task<decimal> CalculateShippingFeeAsync(string province)
        {
            // Giả lập xử lý bất đồng bộ (giống như đang gọi API đơn vị vận chuyển GHN/GHTK)
            await Task.Delay(50);

            if (string.IsNullOrEmpty(province))
            {
                return 0;
            }

            // Logic tính phí ship mẫu: 
            // Nếu là Hà Nội hoặc Hải Phòng thì đồng giá 25.000đ, các tỉnh khác 35.000đ
            if (province.Contains("Hà Nội"))
            {
                return 25000;
            }

            return 35000;
        }
    }
}