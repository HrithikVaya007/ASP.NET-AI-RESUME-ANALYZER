namespace AIResumeAnalyzer.MVC.Helpers
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string Message { get; set; } = string.Empty;

        public static ApiResponse<T> Ok(T data, string message = "")
        {
            return new ApiResponse<T> { Success = true, Data = data, Message = message };
        }

        public static ApiResponse<T> Error(string message)
        {
            return new ApiResponse<T> { Success = false, Data = default, Message = message };
        }
    }
}
