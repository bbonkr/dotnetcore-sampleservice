using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using SampleService.Authorization.Data;
using SampleService.Entities;
using SampleService.Models;
using SampleService.Repositories;

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SampleService.Authorization.App.Services
{
    public interface IUserService
    {
        Task<AuthenticateResponse> AuthenticateAsync(AuthenticateRequest model);
        Task<AuthenticateResponse> RefreshTokenAsync(string token);
        Task<AuthenticateResponse> RevokeTokenAsync(string token);
        Task<IEnumerable<User>> GetAllAsync(int page = 1, int count = 10);

        Task<User> GetByIdAsync(string id);

        Task<AppResponse> CreateAsync(CreateUserRequest model, CancellationToken cancellationToken = default);

        Task<AppResponse> UpdateAsync(UpdateUserRequest user, CancellationToken cancellationToken = default);

        Task<AppResponse> CloseAccountAsync(CloseAccountRequest user, CancellationToken cancellationToken = default);
    }

    public class UserService : IUserService
    {
        //private readonly DataContext dataContext;
        private readonly IUserRepository userRepository;
        private readonly AppSettings appSettings;
        private readonly IHasher hasher;
        private readonly ILogger logger;
        private readonly IHttpContextAccessor httpContextAccessor;

        public UserService(
            //DataContext dataContext,
            IUserRepository userRepository,
            IHasher hasher,
            IHttpContextAccessor httpContextAccessor,
            IOptions<AppSettings> appSettings,
            ILoggerFactory loggerFactory)
        {
            //this.dataContext = dataContext;
            this.userRepository = userRepository;
            this.hasher = hasher;
            this.httpContextAccessor = httpContextAccessor;
            this.appSettings = appSettings.Value;
            this.logger = loggerFactory.CreateLogger<UserService>();
        }

        /// <summary>
        /// Authenticate 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public async Task<AuthenticateResponse> AuthenticateAsync(AuthenticateRequest model)
        {

            string ipAddress = GetIpAddress();

            if (String.IsNullOrWhiteSpace(model.Username))
            {
                logger.LogInformation($"{nameof(AuthenticateRequest.Username)} is empty.");
                throw new ArgumentException("The username is required.", nameof(AuthenticateRequest.Username));
            }
            if (String.IsNullOrWhiteSpace(model.Password))
            {
                logger.LogInformation($"{nameof(AuthenticateRequest.Password)} is empty.");
                throw new ArgumentException("The password is required.", nameof(AuthenticateRequest.Password));
            }

            var user = await userRepository.FindByUsernameAsync(model.Username, true);

            if (user == null)
            {
                return new AuthenticateResponse
                {
                    Status = HttpStatusCode.NotFound,
                    Message = "Check your Username and Password",
                };
            }

            if (user.FailCount > 5)
            {
                return new AuthenticateResponse
                {
                    Status = HttpStatusCode.NotFound,
                    Message = "User account had been locked. Please change user password.",
                };
            }

            if (!hasher.VerifyHashedPassword(user.Password, model.Password))
            {
                await userRepository.UpdateFailCountAsync(user, user.FailCount + 1);

                return new AuthenticateResponse
                {
                    Status = HttpStatusCode.NotFound,
                    Message = "Check your Username and Password",
                };
            }


            await userRepository.BeginTranAsync();
            // 실패횟수 초기화
            await userRepository.ResetFailCountAsync(user);

            var jwtToken = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken(ipAddress);

            // 리프레시 토큰 저장
            await userRepository.AddRefreshTokenAsync(user, refreshToken);

            try
            {

                //await userRepository.CommitAsync();
                await userRepository.CommitAsync();
                logger.LogInformation("Logged in completed");
            }
            catch (Exception ex)
            {
                //await userRepository.RollbackAsync();
                await userRepository.RollbackAsync();
                logger.LogError(ex, "Could not process logged in data.");

                return new AuthenticateResponse
                {
                    Status = HttpStatusCode.InternalServerError,
                    Message = "Could not process logged in data"
                };
            }

            return new AuthenticateResponse
            {
                Status = HttpStatusCode.OK,
                Data = CreateAuthenticateResponse(user, jwtToken, refreshToken.Token),
            };

        }

        public async Task<IEnumerable<User>> GetAllAsync(int page = 1, int count = 10)
        {
            var users = await userRepository.GetUsersAsync(null, page, count, false);

            return users.Select(u => new User
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                UserName = u.UserName,
                Password = "<<This field is not provided for security>>"
            });
        }

        public Task<User> GetByIdAsync(string id)
        {
            return userRepository.FindByIdAsync(id);
        }

        public Task<User> GetByUsernameAsync(string username)
        {
            return userRepository.FindByUsernameAsync(username);
        }

        /// <summary>
        /// Requrest refresh token
        /// </summary>
        /// <param name="token">refresh token</param>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public async Task<AuthenticateResponse> RefreshTokenAsync(string token)
        {
            var ipAddress = GetIpAddress();
            //var user = dataContext.Users
            //    .Include(x => x.RefreshTokens)
            //    //.FirstOrDefault(u => u.RefreshTokens.Any(t => t.Token == token && t.IsActive));
            //    .FirstOrDefault(u => u.RefreshTokens.Any(t => t.Token == token));

            var user = await userRepository.FindByRefreshTokenAsync(token);

            if (user == null)
            {
                return new AuthenticateResponse
                {
                    Status = HttpStatusCode.NotFound,
                    Message = "Could not find a user",
                };
            }

            var refreshToken = user.RefreshTokens.FirstOrDefault(x => x.Token == token);
            if (refreshToken == null)
            {
                return new AuthenticateResponse
                {
                    Status = HttpStatusCode.NotFound,
                    Message = "Could not find a user",
                };
            }

            if (!refreshToken.IsActive)
            {
                return new AuthenticateResponse
                {
                    Status = HttpStatusCode.NotFound,
                    Message = "Token has been expired. Please authenticate again.",
                };
            }

            var jwtToken = GenerateJwtToken(user);
            var newRefreshToken = GenerateRefreshToken(ipAddress);

            refreshToken.Revoked = DateTimeOffset.UtcNow;
            refreshToken.RevokedByIp = ipAddress;
            refreshToken.ReplacedByToken = token;

            await userRepository.BeginTranAsync();


            await userRepository.AddRefreshTokenAsync(user, newRefreshToken);

            await userRepository.RevokeRefreshTokenAsync(user, refreshToken);
            //user.RefreshTokens.Add(newRefreshToken);
            //dataContext.Update(user);

            try
            {
                await userRepository.CommitAsync();
                //await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await userRepository.RollbackAsync();
                //await transaction.RollbackAsync();
                logger.LogError(ex, "Could not save the new refresh token data");
                return new AuthenticateResponse
                {
                    Status = HttpStatusCode.InternalServerError,
                    Message = "Could not save the new refresh token data",
                };
            }

            return new AuthenticateResponse
            {
                Status = HttpStatusCode.OK,
                Data = CreateAuthenticateResponse(user, jwtToken, newRefreshToken.Token),
            };

        }

        /// <summary>
        /// Revoke a refresh token
        /// </summary>
        /// <param name="token">refresh token to Revoke</param>
        /// <param name="ipAddress">ip address</param>
        /// <returns></returns>
        public async Task<AuthenticateResponse> RevokeTokenAsync(string token)
        {
            string ipAddress = GetIpAddress();

            //var user = dataContext.Users
            //    .Include(x => x.RefreshTokens)
            //    .FirstOrDefault(u => u.RefreshTokens.Any(t => t.Token == token ));

            var user = await userRepository.FindByRefreshTokenAsync(token);

            if (user == null)
            {
                return new AuthenticateResponse
                {
                    Status = HttpStatusCode.NotFound,
                    Message = "Could not find a user",
                };
            }

            var refreshToken = user.RefreshTokens.FirstOrDefault(x => x.Token == token);

            if (refreshToken == null)
            {
                return new AuthenticateResponse
                {
                    Status = HttpStatusCode.NotFound,
                    Message = "Could not find a user",
                };
            }

            if (!refreshToken.IsActive)
            {
                return new AuthenticateResponse
                {
                    Status = HttpStatusCode.NotFound,
                    Message = "Could not find a user",
                };
            }

            refreshToken.Revoked = DateTimeOffset.UtcNow;
            refreshToken.RevokedByIp = ipAddress;

            //dataContext.Update(user);


            try
            {
                //dataContext.SaveChanges();
                await userRepository.RevokeRefreshTokenAsync(user, refreshToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Could not save revoked refresh data");
                return new AuthenticateResponse
                {
                    Status = HttpStatusCode.InternalServerError,
                    Message = "Could not save revoked refresh data",
                };
            }

            return new AuthenticateResponse
            {
                Status = HttpStatusCode.Accepted,
                Message = "Token revoked",
            };
        }

        public async Task<AppResponse> CreateAsync(CreateUserRequest model, CancellationToken cancellationToken = default)
        {
            var userFindResult = await GetByUsernameAsync(model.UserName);
            if (userFindResult != null)
            {
                return new CreateUserResponse
                {
                    Message = $"Another user has taken [{model.UserName}]",
                    Status = HttpStatusCode.BadRequest,
                };
            }

            var passwordHashed = hasher.HashPassword(model.Password);
            var user = new User
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                UserName = model.UserName,
                Password = passwordHashed,
                IsEnabled = true,
                IsLocked = false,
                FailCount = 0,
            };

            //dataContext.Add(user);

            //await dataContext.SaveChangesAsync(cancellationToken);

            try
            {
                await userRepository.CreateAsync(user);


                return new CreateUserResponse { Status = HttpStatusCode.Created, };
            }
            catch (Exception)
            {
                return new AppResponse
                {
                    Status = HttpStatusCode.InternalServerError,
                    Message = "Could not process to save a user data.",
                };
            }

        }

        public async Task<AppResponse> UpdateAsync(UpdateUserRequest model, CancellationToken cancellationToken = default)
        {
            var user = await GetByUsernameAsync(model.UserName);

            if (user == null)
            {
                return new AppResponse
                {
                    Status = HttpStatusCode.NotFound,
                    Message = "Could not find a user",
                };
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;

            //dataContext.Update(user);

            //await dataContext.SaveChangesAsync(cancellationToken);

            try
            {
                await userRepository.UpdateAsync(user);

                return new AppResponse { Status = HttpStatusCode.Accepted };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Could not process to save a user data.");
                return new AppResponse
                {
                    Status = HttpStatusCode.InternalServerError,
                    Message = "Could not process to save a user data.",
                };
            }


        }

        public async Task<AppResponse> CloseAccountAsync(CloseAccountRequest model, CancellationToken cancellationToken = default)
        {
            var user = await GetByUsernameAsync(model.UserName);

            if (user != null)
            {
                return new AppResponse
                {
                    Status = HttpStatusCode.NotFound,
                    Message = "Could not find a user",
                };
            }
            //user.IsEnabled = false;

            //dataContext.Update(user);

            //await dataContext.SaveChangesAsync(cancellationToken);

            try
            {
                await userRepository.CloseAccountAsync(user);

                return new AppResponse { Status = HttpStatusCode.Accepted };
            }
            catch (Exception)
            {

                return new AppResponse
                {
                    Status = HttpStatusCode.InternalServerError,
                    Message = "Could not process to save a user data.",
                };
            }

        }

        /// <summary>
        /// Generate JWT token
        /// </summary>
        /// <param name="user">user</param>
        /// <returns></returns>
        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
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
            using (var rngCryptoServiceProvider = new RNGCryptoServiceProvider())
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

        private AuthenticateInnerResponse CreateAuthenticateResponse(User user, string jwtToken, string refreshToken)
        {
            return new AuthenticateInnerResponse
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName,
                JwtToken = jwtToken,
                RefreshToken = refreshToken,
            };
        }

        private string GetIpAddress()
        {
            if (this.httpContextAccessor.HttpContext != null)
            {
                var request = this.httpContextAccessor.HttpContext.Request;

                if (request?.Headers?.ContainsKey("X-Forwarded-For") ?? false)
                {
                    return request.Headers["X-Forwarded-For"];
                }
                else
                {
                    return this.httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.MapToIPv4()?.ToString() ?? "Unknown";
                }
            }

            return "unknown";
        }
    }
}
