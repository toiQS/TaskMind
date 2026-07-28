using Microsoft.AspNetCore.Http;

namespace TaskMind.Applications.Commons
{
    public class ApiResponse
    {
        public int StatusCode { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }

        public static ApiResponse FromServiceResult(ServiceResult result)
        {
            return new ApiResponse
            {
                StatusCode = MapStatusCode(result.Status),
                Status = result.Status.ToString(),
                Message = result.Message,
                Data = null
            };
        }

        public static ApiResponse FromServiceResult<T>(ServiceResult<T> result)
        {
            return new ApiResponse
            {
                StatusCode = MapStatusCode(result.Status),
                Status = result.Status.ToString(),
                Message = result.Message,
                Data = result.Data
            };
        }

        private static int MapStatusCode(ResultStatus status)
        {
            return status switch
            {
                ResultStatus.Success => StatusCodes.Status200OK,
                ResultStatus.NotFound => StatusCodes.Status404NotFound,
                ResultStatus.Failed => StatusCodes.Status400BadRequest,
                ResultStatus.Error => StatusCodes.Status500InternalServerError,
                _ => StatusCodes.Status204NoContent
            };
        }
    }
}