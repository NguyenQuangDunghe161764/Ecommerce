using Microsoft.AspNetCore.Mvc;

namespace demoWebAPI.Models
{
    public abstract class BaseEntity
    {
        public DateTime CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public bool IsDeleted { get; set; }
    }
}
