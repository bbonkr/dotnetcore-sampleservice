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
        protected ObjectResult StatusCodeResult<T>(int status, T data, string message = "")
        {
            return StatusCodeResult((HttpStatusCode)status, data, message);
        }

        protected ObjectResult StatusCodeResult<T>(HttpStatusCode status, T data, string message = "")
        {
         
            return StatusCodeResult(new AppResponse<T>
            {
                Status = status,
                Message = message,
                Data = data,
            });
        }

        protected ObjectResult StatusCodeResult(int status, string message)
        {
            return StatusCodeResult((HttpStatusCode)status, message);
        }

        protected ObjectResult StatusCodeResult(HttpStatusCode status, string message)
        {
            return StatusCodeResult(new AppResponse
            {
                Status = status,
                Message = message,
            });
        }

        protected ObjectResult StatusCodeResult(AppResponse response)
        {
            return StatusCode(response.StatusCode, response);
        }
    }
}
