﻿using BlazorShop.Shared.DTOs;
using BlazorShop.Shared.Entities;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace BlazorShop.Client.Services.OrderService
{
    public class OrderService : IOrderService
    {
        private readonly HttpClient _http;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly NavigationManager _navigationManager;

        public OrderService(HttpClient http,
            AuthenticationStateProvider authStateProvider,
            NavigationManager navigationManager)
        {
            _http = http;
            _authStateProvider = authStateProvider;
            _navigationManager = navigationManager;
        }

        public async Task<OrderDetailsResponseDto> GetOrderDetails(int orderId)
        {
            var result = await _http.GetFromJsonAsync<ServiceResponse<OrderDetailsResponseDto>>($"api/order/{orderId}");
            return result.Data;
        }

        public async Task<List<OrderOverviewResponseDto>> GetOrders()
        {
            var result = await _http.GetFromJsonAsync<ServiceResponse<List<OrderOverviewResponseDto>>>("api/order");
            return result.Data;
        }

        public async Task<string> PlaceOrder()
        {
            if (await IsUserAuthenticated())
            {
                var result = await _http.PostAsync("api/payment/checkout", null);
                var url = await result.Content.ReadAsStringAsync();
                return url;
            }
            else
            {
                return "login";
            }
        }

        //public async Task<bool> FullFillOrder(List<CartProductResponseDto> cartProducts)
        //{
        //    var content = JsonContent.Create(new { user = user });
        //    await _http.PostAsync("api/payment", content);
        //    return true;
        //}
        private async Task<bool> IsUserAuthenticated()
        {
            return (await _authStateProvider.GetAuthenticationStateAsync()).User.Identity.IsAuthenticated;
        }
    }
}
