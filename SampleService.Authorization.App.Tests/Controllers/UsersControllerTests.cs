using System;
using System.Collections.Generic;
using System.Text;

using Castle.Core.Logging;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using Moq;

using SampleService.Authorization.App.Controllers;
using SampleService.Authorization.App.Services;
using SampleService.Authorization.Data;
using SampleService.Entities;
using SampleService.Models;

using Xunit;

namespace SampleService.Authorization.App.Tests.Controllers
{
    public class UsersControllerTests
    {
        [Fact]
        public void Authenticate_should_be_success()
        {
            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(service => service.Authenticate(It.IsAny<AuthenticateRequest>(), It.IsAny<string>()))
                .Returns(GetAuthenticateResponse_Ok());

            var controller = new UsersController(userServiceMock.Object);

            var actionResult = controller.Authenticate(It.IsAny<AuthenticateRequest>());

            var result = Assert.IsType<ObjectResult>(actionResult);
            var resultValue = Assert.IsType<AuthenticateResponse>(result.Value);

            Assert.NotNull(result.Value);
            Assert.True(resultValue.Status == System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public void Authenticate_should_be_fail_unauthorize()
        {
            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(service => service.Authenticate(It.IsAny<AuthenticateRequest>(), It.IsAny<string>()))
                .Returns(GetAuthenticateResponse_Unauthorized());

            var controller = new UsersController(userServiceMock.Object);

            var actionResult = controller.Authenticate(It.IsAny<AuthenticateRequest>());

            var result = Assert.IsType<ObjectResult>(actionResult);
            var resultValue = Assert.IsType<AuthenticateResponse>(result.Value);

            Assert.NotNull(result.Value);
            Assert.True(resultValue.Status == System.Net.HttpStatusCode.Unauthorized);
        }

        private AuthenticateResponse GetAuthenticateResponse_Ok()
        {
            return new AuthenticateResponse
            {
                Status = System.Net.HttpStatusCode.OK,
                Data = new AuthenticateInnerResponse
                {
                    Id = Guid.NewGuid().ToString(),
                    FirstName ="John",
                    LastName = "Doe",
                    UserName="unknownuser",
                    JwtToken = "some-access-token-value",
                    RefreshToken ="some-refresh-token-value",
                },
            };
        }
    
        private AuthenticateResponse GetAuthenticateResponse_Unauthorized()
        {
            return new AuthenticateResponse
            {
                Status = System.Net.HttpStatusCode.Unauthorized,
                Message = "Unauthorized",
            };
        }
    }
}
