using Microsoft.AspNetCore.Mvc;

namespace demoWebAPI.Models
{
    public class Payment
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        public decimal Amount { get; set; }

        public string Status { get; set; }

        public string PaymentMethod { get; set; }

        public DateTime CreatedDate { get; set; }

        public Order Order { get; set; }
    }
}
