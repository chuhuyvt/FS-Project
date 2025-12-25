namespace PlcTagApi.Core.Models.Responses
{
    /// <summary>
    /// Standard API response wrapper
    /// Single Responsibility: Response formatting
    /// </summary>
    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
        public string ErrorCode { get; set; }

        public static ApiResponse SuccessResponse(string message, object data = null)
        {
            return new ApiResponse
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse ErrorResponse(string message, string errorCode = null)
        {
            return new ApiResponse
            {
                Success = false,
                Message = message,
                ErrorCode = errorCode
            };
        }
    }
}
