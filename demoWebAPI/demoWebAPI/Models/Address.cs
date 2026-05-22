using Microsoft.AspNetCore.Mvc;

namespace demoWebAPI.Models
{
    public class Address
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public string FullName { get; set; }

        public string Phone { get; set; }

        public string Street { get; set; }

        public string City { get; set; }

        public string Country { get; set; }

        public bool IsDefault { get; set; }

        public ApplicationUser User { get; set; }
    }
}
