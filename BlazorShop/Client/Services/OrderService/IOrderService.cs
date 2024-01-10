using BlazorShop.Shared.DTOs;

namespace BlazorShop.Client.Services.OrderService
{
    public interface IOrderService
    {
        Task<string> PlaceOrder();
        Task<List<OrderOverviewResponseDto>> GetOrders();
        Task<OrderDetailsResponseDto> GetOrderDetails(int orderId);
    }
}
