﻿using BlazorShop.Shared.Entities;

namespace BlazorShop.Server.Services.AuthService
{
    public interface IAuthService
    {
        Task<ServiceResponse<int>> Register(User user, string password);
        Task<bool> UserExists(string email);
        Task<ServiceResponse<string>> Login(string email, string password);
        Task<ServiceResponse<bool>> Verify(string token);
        Task<ServiceResponse<bool>> ForgotPassword(string email);
        Task<ServiceResponse<bool>> ResetPassword(UserResetPassword resetPassword);
        Task<ServiceResponse<bool>> ChangePassword(int userId, string newPassword);
        int GetUserId();
        string GetUserEmail();
        Task<User> GetUserByEmail(string email);
    }
}
