namespace BLL.DTOs
{
    public class ServiceResult<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
        public List<string>? Errors { get; set; }

        public static ServiceResult<T> SuccessResult(T data, string? message = null)
        {
            return new ServiceResult<T>
            {
                Success = true,
                Data = data,
                Message = message
            };
        }

        public static ServiceResult<T> FailureResult(string message, List<string>? errors = null)
        {
            return new ServiceResult<T>
            {
                Success = false,
                Message = message,
                Errors = errors
            };
        }
    }
}
