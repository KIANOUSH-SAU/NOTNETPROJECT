using System;
using System.ComponentModel.DataAnnotations;

namespace NOTNETPROJECT.Models
{
    public class PendingContactMessage
    {
        [Key]
        public string Code { get; set; } // Unique verification code

        [Required]
        public string Name { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Message { get; set; }

        public DateTime Created { get; set; } = DateTime.UtcNow;
    }
}