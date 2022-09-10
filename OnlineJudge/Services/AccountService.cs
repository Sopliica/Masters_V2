using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlineJudge.Consts;
using OnlineJudge.Database;
using OnlineJudge.Miscs;
using OnlineJudge.Models.Domain;
using OnlineJudge.Models.IO;
using System.Security.Claims;

namespace OnlineJudge.Services
{
    public class AccountService
    {
        private readonly Context context;
        private readonly IPasswordHasher<User> hasher;

        public AccountService(Context context, IPasswordHasher<User> hasher)
        {
            this.context = context;
            this.hasher = hasher;
        }

        public async Task<Result<(ClaimsIdentity Claims, AuthenticationProperties Properties)>> TrySignIn(SignInInput input)
        {
            if (input == null || string.IsNullOrEmpty(input.Email) || string.IsNullOrEmpty(input.Password))
                return Result.Fail<(ClaimsIdentity, AuthenticationProperties)>("Incomplete data");

            var user = await context.Users.FirstOrDefaultAsync(x => x.Email == input.Email);

            if (user == null)
                return Result.Fail<(ClaimsIdentity, AuthenticationProperties)>("Invalid credentials");

            var hashResult = hasher.VerifyHashedPassword(user, user.PasswordHash, input.Password);

            if (hashResult != PasswordVerificationResult.Success)
            {
                return Result.Fail<(ClaimsIdentity, AuthenticationProperties)>("Invalid credentials");
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                RedirectUri = "/"
            };

            return Result.Ok((claimsIdentity, authProperties));
        }

        public async Task<Result<(ClaimsIdentity Claims, AuthenticationProperties Properties)>> TryRegister(SignInInput input)
        {
            if (input == null || string.IsNullOrEmpty(input.Email) || string.IsNullOrEmpty(input.Password))
                return Result.Fail<(ClaimsIdentity, AuthenticationProperties)>("Incomplete data");

            var user = context.Users.FirstOrDefault(x => x.Email.ToLower() == input.Email.ToLower());

            if (user != null)
                return Result.Fail<(ClaimsIdentity, AuthenticationProperties)>("User with this email already exists");

            user = new User(input.Email, Roles.User);
            user.PasswordHash = hasher.HashPassword(user, input.Password);

            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                RedirectUri = "/"
            };

            return Result.Ok((claimsIdentity, authProperties));
        }
    }
}
