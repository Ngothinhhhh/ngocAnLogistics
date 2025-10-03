namespace WebApplication3.DTOs.ActivityDTOs
{
    public class ActivityForCreation
    {
        public int OrderID { get; set; }
        public int UserID { get; set; }
        public string FieldName { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public string IP { get; set; }
        public string? ActivityDetail { get; set; }
        //public DateTime ActivityDate { get; set; }


    }
}
