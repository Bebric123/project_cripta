using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace project_cripta.Models
{
    public class UserBalance
    {
        public int Id { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required]
        [StringLength(10)]
        public string Currency { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public virtual User User { get; set; }
    }
}