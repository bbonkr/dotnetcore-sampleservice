using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using SampleService.Authorization.App.Services;
using SampleService.Models;

namespace SampleService.Authorization.App.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controlller]")]
    public class UsersController:ControllerBase
    {
        private readonly IUserService userService;

        public UsersController(IUserService userService)
        {
            this.userService = userService;
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody] AuthenticateRequest model)
        {
            var response = userService.Authenticate(model, GetIpAddress());

            if(response == null)
            {
                return BadRequest(new MessageResponse
                {
                    Message = "Check your Username and Password",
                });
            }

            SetTokenCookie(response.RefreshToken);

            return Ok(response);
        }

        [AllowAnonymous]
        [HttpPost("refresh-token")]
        public IActionResult RefreshToken([FromBody] RefreshTokenRequest model)
        {
            var token = model.Token ?? Request.Cookies["refreshToken"];

            if(String.IsNullOrWhiteSpace(token))
            {
                return BadRequest(new MessageResponse
                {
                    Message = "Token is Required",
                });
            }

            var response = userService.RefreshToken(token, GetIpAddress());

            if(response == null)
            {
                return Unauthorized(new MessageResponse
                {
                    Message = "Invalid token",
                });
            }

            SetTokenCookie(response.RefreshToken);

            return Ok(response);
        }

        public IActionResult Revoke([FromBody] RevokeTokenRequest model)
        {
            var token = model.Token ?? Request.Cookies["refreshToken"];


            if (String.IsNullOrWhiteSpace(token))
            {
                return BadRequest(new MessageResponse
                {
                    Message = "Token is Required",
                });
            }

            var result = userService.RevokeToken(token, GetIpAddress());

            if (!result)
            {
                return NotFound(new MessageResponse
                {
                    Message = "Token not found",
                });
            }

            return Ok(new MessageResponse
            {
                Message = "Token is revoked",
            });
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var users = userService.GetAll();

            return Ok(users);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(string id)
        {
            var user = userService.GetById(id);
            if (user == null)
            {
                return NotFound(new MessageResponse
                {
                    Message = "Could not find a user"
                });
            }

            return Ok(user);
        }

        [HttpGet("{id}/refresh-tokens")]
        public IActionResult GetRefreshTokens(string id)
        {
            var user = userService.GetById(id);
            if(user == null)
            {
                return NotFound(new MessageResponse
                {
                    Message = "Could not find a user"
                });
            }

            return Ok(user.RefreshTokens);
        }

        private void SetTokenCookie(string token)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTimeOffset.UtcNow.AddDays(7),
            };

            Response.Cookies.Append("refreshToken", token, cookieOptions);
        }

        private string GetIpAddress()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                return Request.Headers["X-Forwarded-For"];
            }
            else
            {
                return HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
        }
    }
}
