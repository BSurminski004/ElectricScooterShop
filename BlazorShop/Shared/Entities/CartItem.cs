namespace BlazorShop.Shared.Entities
{
    public class CartItem
    {
        //public User User { get; set; }
        public int UserId { get; set; }
        //public Product Product { get; set; }
        public int ProductId { get; set; }
        //public ProductType MyProperty { get; set; }
        public int ProductTypeId { get; set; }
        public int Quantity { get; set; } = 1;
    }
}
