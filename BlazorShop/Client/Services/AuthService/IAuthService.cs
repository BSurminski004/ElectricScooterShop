using BlazorShop.Shared.Entities;

namespace BlazorShop.Client.Services.AuthService
{
    public interface IAuthService
    {
        Task<ServiceResponse<int>> Register(UserRegister request);
        Task<ServiceResponse<string>> Login(UserLogin request);
        Task<ServiceResponse<bool>> ChangePassword(UserChangePassword request);
        Task<ServiceResponse<bool>> ForgotPassword(string email);
        Task<ServiceResponse<bool>> ResetPassword(UserResetPassword request);
        Task<bool> IsUserAuthenticated();
    }
}
