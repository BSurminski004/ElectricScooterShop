using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorShop.Shared.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public string DeliveryType { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal DeliveryPrice { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal ItemPrice { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }
        public List<OrderItem> OrderItems { get; set; }
    }
}
