namespace WebApplication3.DTOs.CustomerDTOs
{
    public class CreateCustomerDTO
    {
        
        public int UserID { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public string CustomerAddr { get; set; }
        public int DisplayOrder { get; set; }
        public string TaxNo { get; set; }
        public string ContactName { get; set; }
        public string ContactPhone { get; set; }
    }
}
