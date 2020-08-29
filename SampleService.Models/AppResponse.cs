using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace SampleService.Models
{
    public class AppResponse
    {
        public string Message { get; set; }

        public HttpStatusCode Status { get; set; }

        /// <summary>
        /// 요청 성공여부
        /// </summary>
        public bool IsSuccessful
        {
            get => 200 >= StatusCode && StatusCode < 300;
        }

        public int StatusCode
        {
            get => (int)Status;
        }
    }

    public class AppResponse<T> : AppResponse
    {
        public T Data { get; set; }
    }
}
