using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Mvc;
using System.Configuration;
using WebApplication3.DTOs;

namespace WebApplication3.Helpers
{
    public class ValidateString
    {
        public static ApiResponse<string> StringValidator(Dictionary<string,string> valueA)
        {
            foreach (KeyValuePair<string, string> entry in valueA)
            {
                if (string.IsNullOrWhiteSpace(entry.Value))
                {
                    return new ApiResponse<string> { statusCode = 400, Message = $"⚠️ {entry.Key} không được để trống.", Data = null };
                }
            }
            return null;
        }
    }
}
