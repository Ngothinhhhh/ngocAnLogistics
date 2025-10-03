namespace WebApplication3.DTOs
{
    public class ApiResponse<T>
    {
        public int statusCode { get; set; }   
        public string Message { get; set; }
        public T Data { get; set; }
    }
}
