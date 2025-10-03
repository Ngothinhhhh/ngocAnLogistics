namespace WebApplication3.DTOs.ImageDTOs
{
    public class PhotoForCreationDTO
    {
        public string OrderID { get; set; } 
        //public string FileName { get; set; }
        public IFormFile File { get; set; }
        //public string URl { get; set; }
        public string? Descrip { get; set; }
        //public int UserID { get; set; }
    }
}
