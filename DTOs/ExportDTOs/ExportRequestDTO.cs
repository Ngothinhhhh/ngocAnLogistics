using Microsoft.AspNetCore.Mvc;
using WebApplication3.Models;

namespace WebApplication3.DTOs.RequestDTOs
{
    public class ExportRequestDTO
    {
        public string? fromDateStr {  get; set; }
        public string? toDateStr { get; set; }
        public string order { get; set; } = "asc";
        public string sortBy { get; set; } = "id";
        public int pageSize { get; set; } = 30;
        public int pageNumber { get; set; } = 1;
        public string? searchKey { get; set; }
        public string? status {  get; set; }
    }
}
