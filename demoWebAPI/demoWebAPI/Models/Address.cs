using Microsoft.AspNetCore.Mvc;

namespace demoWebAPI.Models
{
public class Address
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public string FullName { get; set; }

        public string PhoneNumber { get; set; }

        public string Province { get; set; }

        public string District { get; set; }

        public string Ward { get; set; }

        public string DetailAddress { get; set; }

        public bool IsDefault { get; set; }

        public DateTime CreatedDate { get; set; }

        // NAVIGATION

        public virtual ApplicationUser User { get; set; }
    }
}
