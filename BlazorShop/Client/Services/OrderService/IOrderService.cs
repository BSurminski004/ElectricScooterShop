using BlazorShop.Shared.DTOs;

namespace BlazorShop.Client.Services.OrderService
{
    public interface IOrderService
    {
        Task<string> PlaceOrder();
        //Task<string> FullFillOrder(List<CartProductResponseDto> cartProducts);
        Task<List<OrderOverviewResponseDto>> GetOrders();
        Task<OrderDetailsResponseDto> GetOrderDetails(int orderId);
    }
}
