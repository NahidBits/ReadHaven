using System;
using System.ComponentModel.DataAnnotations;
using ReadHaven.Models.Common;
using ReadHaven.Models.Enums;

namespace ReadHaven.Models
{
    public class PaymentTransaction : BaseEntity
    {
        public Guid OrderId { get; set; }  
        public decimal TotalAmount { get; set; }
        public Currency Currency { get; set; }
        public PaymentMethod PaymentMethod { get; set; } 
        public string TransactionId { get; set; } = $"TXN-{DateTime.UtcNow:yyyyMMddHHmmss}-{new Random().Next(1000, 9999)}";
        public Status Status { get; set; }
        public decimal DiscountAmount { get; set; } = 5;
        public decimal TaxAmount { get; set; } = 10;
    }
}