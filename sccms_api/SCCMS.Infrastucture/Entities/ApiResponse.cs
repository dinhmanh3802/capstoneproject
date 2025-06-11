using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SCCMS.Infrastucture.Entities
{
    public class ApiResponse
    {
        // Constructor mặc định
        public ApiResponse()
        {
            ErrorMessages = new List<string>();
        }

        // Constructor có tham số cho StatusCode và IsSuccess
        public ApiResponse(HttpStatusCode statusCode, bool isSuccess)
        {
            StatusCode = statusCode;
            IsSuccess = isSuccess;
            ErrorMessages = new List<string>();
        }

        // Constructor có tham số cho StatusCode, IsSuccess và Result
        public ApiResponse(HttpStatusCode statusCode, bool isSuccess, object? result)
        {
            StatusCode = statusCode;
            IsSuccess = isSuccess;
            Result = result;
            ErrorMessages = new List<string>();
        }

        // Constructor có tham số cho StatusCode, IsSuccess và ErrorMessages
        public ApiResponse(HttpStatusCode statusCode, bool isSuccess, List<string> errorMessages)
        {
            StatusCode = statusCode;
            IsSuccess = isSuccess;
            ErrorMessages = errorMessages ?? new List<string>();
        }

        public HttpStatusCode StatusCode { get; set; }
        public bool IsSuccess { get; set; }
        public List<string> ErrorMessages { get; set; }
        public object? Result { get; set; }

    }
}
