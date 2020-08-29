using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Protocol;

using SampleService.Models;

namespace SampleService.Authorization.App
{
    public class AppApiController: ControllerBase
    {
        protected ObjectResult StatusCodeResult(HttpStatusCode status, string message)
        {
            return StatusCodeResult((int)status, message);
        }
        protected ObjectResult StatusCodeResult(int status, string message)
        {
            return StatusCode(status, new MessageResponse
            {
                Message = message,
            });
        }

        protected ObjectResult StatusCodeResult(AppResponse response)
        {
            return StatusCodeResult(response.StatusCode, response.Message);
        }
    }
}
