using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Server.Models;
using SharedLibrary;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Server.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly Settings settings;
        private readonly GameDBContext context;

        public AuthenticationService(Settings settings, GameDBContext context)
        {
            this.settings = settings;
            this.context = context;
        }
        public (bool success, string content) Register(string username, string password)
        {
            if (context.Users.Any(u => u.Username == username)) return (false, "Username not avialable");

            var user = new User { Username = username, PasswordHash = password };
            user.ProvideSaltAndHash();

            context.Add(user);
            context.SaveChanges();

            return (true, "");
        }

        public (bool success, string token) Login(string username, string password)
        {
            var user = context.Users.Include(u=>u.Heroes).SingleOrDefault(u => u.Username == username);
            if (user == null) return (false, "Invalid username");

            if (user.PasswordHash != AuthenticationHelpers.ComputeHash(password, user.Salt)) return (false, "Invalid password");

            return (true, GenerateJWTToken(AssembleClaimsIdentity(user)));
        }

        private ClaimsIdentity AssembleClaimsIdentity(User user)
        {
            var subject = new ClaimsIdentity(new[]
            {
                new Claim("id", user.Id.ToString()),
                new Claim("heroes",JsonConvert.SerializeObject(user.Heroes.Select(h=>h.Id)))
            });

            return subject;
        }
        private string GenerateJWTToken(ClaimsIdentity subject)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(settings.BearerKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = subject,
                Expires = DateTime.Now.AddYears(10),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
    public interface IAuthenticationService
    {
        (bool success, string content) Register(string username, string password);
        (bool success, string token) Login(string username, string password);
    }
    public static class AuthenticationHelpers
    {
        public static void ProvideSaltAndHash(this User user)
        {
            var salt = GenerateSalt();
            user.Salt = Convert.ToBase64String(salt);
            user.PasswordHash = ComputeHash(user.PasswordHash, user.Salt);
        }
        private static byte[] GenerateSalt()
        {
            var rng = RandomNumberGenerator.Create();
            var salt = new byte[24];
            rng.GetBytes(salt);
            return salt; 
        }
        public static string ComputeHash(string password, string saltString)
        {
            var salt = Convert.FromBase64String(saltString);

            using var hashGenerator = new Rfc2898DeriveBytes(password, salt, 10101, HashAlgorithmName.SHA1);
            var bytes = hashGenerator.GetBytes(24);
            return Convert.ToBase64String(bytes);
        }
    }
}
