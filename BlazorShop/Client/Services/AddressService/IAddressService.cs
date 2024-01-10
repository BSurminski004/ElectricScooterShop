using BlazorShop.Shared.Entities;
using System.Net;

namespace BlazorShop.Client.Services.AddressService
{
    public interface IAddressService
    {
        Task<Address> GetAddress();
        Task<Address> AddOrUpdateAddress(Address address);
    }
}
