using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
    [Route("api/[controller]")]
    public class UsersController:AppApiController
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

            if (!response.IsSuccessful)
            {
                return StatusCodeResult(response);
            }

            SetTokenCookie(response.Data.RefreshToken);

            return StatusCodeResult(response);
        }

        [AllowAnonymous]
        [HttpPost("refresh-token")]
        public IActionResult RefreshToken([FromBody] RefreshTokenRequest model)
        {
            var token = model.Token ?? Request.Cookies["refreshToken"];

            if (String.IsNullOrWhiteSpace(token))
            {
                return StatusCodeResult(HttpStatusCode.BadRequest, "Token is required.");
            }

            var response = userService.RefreshToken(token, GetIpAddress());

            if (!response.IsSuccessful)
            {

                return StatusCodeResult(HttpStatusCode.Unauthorized, "Invalid token");
            }

            SetTokenCookie(response.Data.RefreshToken);

            return StatusCodeResult(response);
        }

        [HttpPost("revoke-token")]
        public IActionResult Revoke([FromBody] RevokeTokenRequest model)
        {
            var token = model.Token ?? Request.Cookies["refreshToken"];

            if (String.IsNullOrWhiteSpace(token))
            {

                return StatusCodeResult(HttpStatusCode.BadRequest, "Token is Required");
            }

            var response = userService.RevokeToken(token, GetIpAddress());

            return StatusCodeResult(response);
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var users = userService.GetAll();

            return StatusCodeResult(HttpStatusCode.OK, users);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(string id)
        {
            var status = HttpStatusCode.OK;
            var message = "";

            var user = userService.GetById(id);
            if (user == null)
            {
                status = HttpStatusCode.NotFound;
                message = "Could not find a user";
            }

            return StatusCodeResult(status, user, message);
        }

        [HttpGet("{id}/refresh-tokens")]
        public IActionResult GetRefreshTokens(string id)
        {
            var user = userService.GetById(id);
            if(user == null)
            {
                return StatusCodeResult(HttpStatusCode.NotFound, "Could not find a user");
            }

            return StatusCodeResult(HttpStatusCode.OK, user.RefreshTokens);
        }

        [AllowAnonymous]
        [HttpPut]
        public async Task<IActionResult> CreateUserAsync([FromBody] CreateUserRequest model)
        {
            if (!ModelState.IsValid)
            {
                return StatusCodeResult(HttpStatusCode.BadRequest, "Invalid request body");
            }

            var response = await userService.CreateAsync(model);

            return StatusCodeResult(response);
        }

        private void SetTokenCookie(string token)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTimeOffset.UtcNow.AddDays(7),
            };

            Response?.Cookies.Append("refreshToken", token, cookieOptions);
        }

        private string GetIpAddress()
        {
            if (Request?.Headers?.ContainsKey("X-Forwarded-For") ?? false)
            {
                return Request.Headers["X-Forwarded-For"];
            }
            else
            {
                return HttpContext?.Connection?.RemoteIpAddress?.MapToIPv4()?.ToString() ?? "Unknown";
            }
        }
    }
}
