using BlazorShop.DataAccess.DataContext;
using BlazorShop.Shared.Entities;
using MailKit.Net.Smtp;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace BlazorShop.Server.Services.AuthService
{
    public class AuthService : IAuthService
    {
        private readonly DataContext _context;
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(DataContext context,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _config = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        //zmienna ktora wyrzuca blad!! Nie widzi CLaimTypesow -> nie wylogowuje od razu dlatego
        public int GetUserId() => int.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));

        public string GetUserEmail() => _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Name);

        public async Task<ServiceResponse<string>> Login(string email, string password)
        {
            var response = new ServiceResponse<string>();
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Email.ToLower().Equals(email.ToLower()));
            if (user == null || !VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
            {
                response.Success = false;
                response.Message = "Nie znaleziono użytkownika lub wpisano niepoprawne hasło.";
            }
            else if (user.VerifiedTime == null)
            {
                response.Success = false;
                response.Message = "Użytkownik nie został zweryfikowany!";
            }
            else
            {
                response.Data = CreateToken(user);
            }

            return response;
        }

        public async Task<ServiceResponse<int>> Register(User user, string password)
        {
            if (await UserExists(user.Email))
            {
                return new ServiceResponse<int>
                {
                    Success = false,
                    Message = "Użykownik już istnieje."
                };
            }

            CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            user.VerificationToken = CreateVerifyToken();
            user.VerifiedTime = null;

            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            SendEmail(user.Email, $"Witaj w BSE Shop. Potwierdź swoje konto.", VerifyAccountMessage(user.VerificationToken, user.Email));

            return new ServiceResponse<int> { Data = user.Id, Message = "Zostałeś zrejestrowany, zweryfikuj swoje konto!" };
        }

        public async Task<ServiceResponse<bool>> Verify(string token)
        {
            var response = new ServiceResponse<bool>();
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.VerificationToken == token);

            if (user == null)
            {
                response.Success = false;
                response.Message = "Nie znaleziono użytkownika.";
                return response;
            }

            user.VerifiedTime = DateTime.Now;
            await _context.SaveChangesAsync();

            return new ServiceResponse<bool> { Data = true, Message = "Użytkownik został zweryfikowany." };

        }

        public async Task<bool> UserExists(string email)
        {
            if (await _context.Users.AnyAsync(user => user.Email.ToLower()
                 .Equals(email.ToLower())))
            {
                return true;
            }
            return false;
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordS)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordS = hmac.Key;
                passwordHash = hmac
                    .ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private string CreateVerifyToken()
        {
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash =
                    hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }

        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8
                .GetBytes(_config.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.Now.AddDays(1),
                    signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        public async Task<ServiceResponse<bool>> ChangePassword(int userId, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = "Nie znaleziono użytkownika."
                };
            }

            CreatePasswordHash(newPassword, out byte[] passwordHash, out byte[] passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            await _context.SaveChangesAsync();

            return new ServiceResponse<bool> { Data = true, Message = "Hasło zostało zmienione." };
        }

        public async Task<ServiceResponse<bool>> ForgotPassword(string email)
        {
            var response = new ServiceResponse<bool>();
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Email == email);

            if (user == null)
            {
                response.Success = false;
                response.Message = "Email restujacy haslo został wyslany.";
                return response;
            }

            user.PasswordResetToken = CreateVerifyToken();
            user.ResetTokenExpires = DateTime.Now.AddDays(1);
            SendEmail(user.Email, $"Zresetuj hasło", ForgotPasswordMessage(user.PasswordResetToken, user.Email));
            await _context.SaveChangesAsync();

            return new ServiceResponse<bool> { Data = true, Message = "Hasło zostało zmienione." };
        }

        public async Task<ServiceResponse<bool>> ResetPassword(UserResetPassword resetPassword)
        {
            var response = new ServiceResponse<bool>();
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.PasswordResetToken == resetPassword.Token);

            if (user == null || user.ResetTokenExpires < DateTime.Now)
            {
                response.Success = false;
                response.Message = "Sesja resetujaca haslo wygasła.";
                return response;
            }

            CreatePasswordHash(resetPassword.Password, out byte[] passwordHash, out byte[] passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            user.PasswordResetToken = null;
            user.ResetTokenExpires = null;

            await _context.SaveChangesAsync();
            return new ServiceResponse<bool> { Data = true, Message = "Hasło zostało zresetowane." }; ;
        }

        public async Task<User> GetUserByEmail(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email.Equals(email));
        }

        private void SendEmail(string senderEmail, string subject, string body)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("BS Scooter", _config.GetSection("EmailConfig:EmailUserName").Value));
            email.To.Add(MailboxAddress.Parse(senderEmail));
            email.Subject = subject;
            email.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = body };

            using var smtp = new SmtpClient();
            smtp.Connect(_config.GetSection("EmailConfig:EmailHost").Value, 587, MailKit.Security.SecureSocketOptions.StartTls);
            smtp.Authenticate(_config.GetSection("EmailConfig:EmailUserName").Value, _config.GetSection("EmailConfig:EmailPassword").Value);
            smtp.Send(email);
            smtp.Disconnect(true);
        }

        private string VerifyAccountMessage(string verificationToken, string userEmail)
        {
            string url = $"https://localhost:7111/api/Auth/verify?token={verificationToken}";
            //string message = @$"Witaj {userEmail}!!,
            //Miło nam, że chcesz dołączyć do grona naszych klientów.
            //Aktywuj swoje konto by móc w pełni korzystać w usług BSE Shop
            //{url}
            //Dobrze, że jesteś z nami ;)
            //-- BS Shop Team";
            string message = $"<h3> Witaj {userEmail}!!</h3><p>Miło nam, że chcesz dołączyć do grona naszych klientów.</p> <p>Aktywuj swoje konto by móc w pełni korzystać w usług BSE Shop</p> <a href=\"{url}\">Zweryfikuj konto</a> <p>Dobrze, że jesteś z nami ;)</p> <p>-- BS Shop Team</p>";

            return message;
        }

        private string ForgotPasswordMessage(string resetPasswordToken, string userEmail)
        {
            string url = $"https://localhost:7111/api/Auth/reset-password?token={resetPasswordToken}";
            string message = @$"Restetowanie hasła {userEmail}!!,

           Link do zresetowania hasła:

            {url}

            -- BS Shop Team";

            return message;
        }
    }
}
