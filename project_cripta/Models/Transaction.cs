using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace project_cripta.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required]
        [StringLength(10)]
        public string Type { get; set; }
        [Required]
        [StringLength(10)]
        public string FromCurrency { get; set; }
        [Required]
        [StringLength(10)]
        public string ToCurrency { get; set; }
        public decimal FromAmount { get; set; }
        public decimal ToAmount { get; set; }
        public decimal ExchangeRate { get; set; }
        public decimal Fee { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public virtual User User { get; set; }
    }
}