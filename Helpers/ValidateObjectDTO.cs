using WebApplication3.DTOs;

namespace WebApplication3.Helpers
{
    public static class ValidateObjectDTO
    {
        public static ApiResponse<Object> StringValidator(Dictionary<string, string> valueA)
        {
            foreach (KeyValuePair<string, string> entry in valueA)
            {
                if (string.IsNullOrWhiteSpace(entry.Value))
                {
                    return new ApiResponse<Object> { statusCode = 400, Message = $"⚠️ {entry.Key} không được để trống.", Data = null };
                }
            }
            return null;
        }
    }
}
