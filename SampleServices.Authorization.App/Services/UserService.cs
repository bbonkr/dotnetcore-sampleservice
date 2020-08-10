using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using SampleServices.Authorization.Data;
using SampleServices.Entities;
using SampleServices.Models;

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SampleServices.Authorization.App.Services
{
    public interface IUserService
    {
        AuthenticateResponse Authenticate(AuthenticateRequest model, string ipAddress);
        AuthenticateResponse RefreshToken(string token, string ipAddress);
        bool RevokeToken(string token, string ipAddress);
        IEnumerable<User> GetAll();
        User GetById(string id);
    }

    public class UserService : IUserService
    {
        private readonly DataContext dataContext;
        private readonly AppSettings appSettings;
        private readonly IHasher hasher;
        private readonly ILogger logger;

        public UserService(
            DataContext dataContext,
            IHasher hasher,
            IOptions<AppSettings> appSettings,
            ILoggerFactory loggerFactory)
        {
            this.dataContext = dataContext;
            this.hasher = hasher;
            this.appSettings = appSettings.Value;
            this.logger = loggerFactory.CreateLogger<UserService>();
        }

        /// <summary>
        /// Authenticate 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public AuthenticateResponse Authenticate(AuthenticateRequest model, string ipAddress)
        {
            if (String.IsNullOrWhiteSpace(model.UserName))
            {
                logger.LogInformation($"{nameof(AuthenticateRequest.UserName)} is empty.");
                throw new ArgumentException("The username is required.", nameof(AuthenticateRequest.UserName));
            }
            if (String.IsNullOrWhiteSpace(model.Password))
            {
                logger.LogInformation($"{nameof(AuthenticateRequest.Password)} is empty.");
                throw new ArgumentException("The password is required.", nameof(AuthenticateRequest.Password));
            }

            var user = dataContext.Users.Where(x => x.UserName == model.UserName).FirstOrDefault();

            if (user == null)
            {
                return null;
            }

            if (!hasher.Verify(user.Password, model.Password))
            {
                return null;
            }
            var jwtToken = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken(ipAddress);

            user.RefreshTokens.Add(refreshToken);
            dataContext.Update(user);
            try
            {
                dataContext.SaveChanges();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Could not save token data.");
                return null;
            }

            return CreateAuthenticateResponse(user, jwtToken, refreshToken.Token);
        }

        public IEnumerable<User> GetAll()
        {
            return dataContext.Users.Select(u => new User
            {
                Id = u.Id,
                FirstName=u.FirstName,
                LastName=u.LastName,
                UserName=u.UserName,
                Password = "<<This field is not provided for security>>"
            });
        }

        public User GetById(string id)
        {
            return dataContext.Users.FirstOrDefault(x => x.Id == id);
        }

        /// <summary>
        /// Requrest refresh token
        /// </summary>
        /// <param name="token">refresh token</param>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public AuthenticateResponse RefreshToken(string token, string ipAddress)
        {
            var user = dataContext.Users.SingleOrDefault(u => u.RefreshTokens.Any(t => t.Token == token));

            if(user == null)
            {
                return null;
            }

            var refreshToken = user.RefreshTokens.FirstOrDefault(x => x.Token == token);
            if(refreshToken == null)
            {
                return null;
            }

            if (!refreshToken.IsActive)
            {
                return null;
            }

            var jwtToken = GenerateJwtToken(user);
            var newRefreshToken = GenerateRefreshToken(ipAddress);
            
            refreshToken.Revoked = DateTimeOffset.UtcNow;
            refreshToken.RevokedByIp = ipAddress;
            refreshToken.ReplacedByToken = ipAddress;

            user.RefreshTokens.Add(newRefreshToken);
            dataContext.Update(user);

            try
            {
                dataContext.SaveChanges();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Could not save the new refresh token data");
                return null;
            }

            return CreateAuthenticateResponse(user, jwtToken, newRefreshToken.Token);
        }

        /// <summary>
        /// Revoke a refresh token
        /// </summary>
        /// <param name="token">refresh token to Revoke</param>
        /// <param name="ipAddress">ip address</param>
        /// <returns></returns>
        public bool RevokeToken(string token, string ipAddress)
        {
            var user = dataContext.Users.SingleOrDefault(u => u.RefreshTokens.Any(t => t.Token == token));

            if(user == null)
            {
                return false;
            }

            var refreshToken = user.RefreshTokens.FirstOrDefault(x => x.Token == token );

            if (refreshToken == null)
            {
                return false;
            }

            if (!refreshToken.IsActive)
            {
                return false;
            }

            refreshToken.Revoked = DateTimeOffset.UtcNow;
            refreshToken.RevokedByIp = ipAddress;

            dataContext.Update(user);

            try
            {
                dataContext.SaveChanges();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Could not save revoked refresh data");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Generate JWT token
        /// </summary>
        /// <param name="user">user</param>
        /// <returns></returns>
        private string GenerateJwtToken (User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor {
                Subject = new ClaimsIdentity(new Claim[] {
                    new Claim(ClaimTypes.Name, user.Id) 
                }),
                Expires = DateTimeOffset.UtcNow.AddMinutes(15).UtcDateTime,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            
            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Generate refresh token
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        private RefreshToken GenerateRefreshToken(string ipAddress)
        {
            using (var rngCryptoServiceProvider=new RNGCryptoServiceProvider())
            {
                var randomBytes = new byte[64];
                rngCryptoServiceProvider.GetBytes(randomBytes);

                return new RefreshToken
                {
                    Token = Convert.ToBase64String(randomBytes),
                    Expires = DateTimeOffset.UtcNow.AddDays(7),
                    Created = DateTimeOffset.UtcNow,
                    CreatedByIp = ipAddress,
                };
            }
        }

        private AuthenticateResponse CreateAuthenticateResponse(User user, string jwtToken, string refreshToken)
        {
            return new AuthenticateResponse
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName,
                JwtToken = jwtToken,
                RefreshToken = refreshToken,
            };
        }
    }
}
