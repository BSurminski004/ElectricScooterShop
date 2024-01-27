using BlazorShop.Shared.Entities;

namespace BlazorShop.Server.Services.EmailService
{
    public interface IEmailService
    {
        void SendEmail(Email request);
    }
}
