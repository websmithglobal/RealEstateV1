namespace RealEstate.Application.DTOs
{
    /// <summary>
    /// Represents a data transfer object for user master details.
    /// Created By - Nirmal
    /// Created Date - 12.11.2025
    /// </summary>
    public class UserMasterDTO
    {
        public int SrNo { get; set; }
        public int UserIDP { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string RoleName { get; set; }
        public bool IsActive { get; set; }
        public string CreatedDate { get; set; }
    }

    /// <summary>
    /// Represents a paging response for user master entities.
    /// Created By - Nirmal
    /// Created Date - 12.11.2025
    /// </summary>
    public class UserMasterEntityPaging
    {
        public List<UserMasterDTO> Records { get; set; }
        public int TotalRecord { get; set; }
    }
}
