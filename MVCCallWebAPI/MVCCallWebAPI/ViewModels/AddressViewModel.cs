using System.ComponentModel.DataAnnotations;

namespace MVCCallWebAPI.ViewModels
{
    public class AddressViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên người nhận")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn Tỉnh/Thành")]
        public string Province { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn Quận/Huyện")]
        public string District { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn Phường/Xã")]
        public string Ward { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số nhà, tên đường")]
        public string DetailAddress { get; set; }

        public bool IsDefault { get; set; }

        public string FullAddress => $"{DetailAddress}, {Ward}, {District}, {Province}";
    }
}