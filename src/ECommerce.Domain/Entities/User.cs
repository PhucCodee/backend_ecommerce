using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities
{
    public class User : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string Username { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; }

        [Required]
        [StringLength(256)]
        public string PasswordHash { get; set; }

        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}