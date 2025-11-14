using System.ComponentModel.DataAnnotations;

namespace RealEstate.Application.DTOs
{
    /// <summary>
    /// Represents a data transfer object for property type details.
    /// Created By - Prashant
    /// Created Date - 14.11.2025
    /// </summary>
    public class PropertyTypeMasterDTO
    {
        public int SrNo { get; set; }
        public int PropertyTypeIDP { get; set; }
        public string PropertyTypeName { get; set; }
        public bool IsActive { get; set; }
        public Guid UserIDF { get; set; }
        public string CreatedDate { get; set; }
    }

    /// <summary>
    /// Represents a paging response for property type entities.
    /// Created By - Prashant
    /// Created Date - 14.11.2025
    /// </summary>
    public class PropertyTypeMasterPagingDTO
    {
        public List<PropertyTypeMasterDTO> Records { get; set; }
        public int TotalRecord { get; set; }
    }

    /// <summary>
    /// Represents a data transfer object for saving or updating property type details.
    /// Created By - Prashant
    /// Created Date - 14.11.2025
    /// </summary>
    public class PropertyTypeMasterSaveDTO
    {
        public int? PropertyTypeIDP { get; set; }

        [Required(ErrorMessage = "Please enter Property Type Name.")]
        [StringLength(150, ErrorMessage = "Property Type Name cannot exceed 150 characters.")]
        [MinLength(3, ErrorMessage = "Property Type Name must be at least 3 characters long.")]
        public string PropertyTypeName { get; set; }
        public Guid UserIDF { get; set; }
    }


}
