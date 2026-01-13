using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace project_cripta.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public decimal Balance { get; set; } = 10000;

        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<UserBalance> Balances { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }
    }
}