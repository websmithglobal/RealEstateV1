namespace RealEstate.Application.DTOs.CommonDTOs
{
    /// <summary>
    /// Represents a data transfer object for common paging and sorting parameters, typically used in grid or table pagination scenarios.
    /// Created By - Nirmal
    /// Created Date - 12.11.2025
    /// </summary>
    public class CommonPagingDTO
    {
        public int DisplayLength { get; set; } = 10;
        public int DisplayStart { get; set; } = 0;
        public string? OrderByFieldName { get; set; }
        public int OrderByType { get; set; } = 0;
        public string? SearchValue { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
    }

}
