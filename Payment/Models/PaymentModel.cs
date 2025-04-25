using System;
using System.ComponentModel.DataAnnotations;

namespace Payment.Models
{
    public class PaymentModel : BaseEntity
    {
        public decimal Amount { get; set; }
        public Currency Currency { get; set; }
        public PaymentMethod PaymentMethod { get; set; } 
        public string TransactionId { get; set; } = $"TXN-{DateTime.UtcNow:yyyyMMddHHmmss}-{new Random().Next(1000, 9999)}";
        public Status Status { get; set; } 
    }
}