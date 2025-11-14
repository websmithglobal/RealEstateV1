using System.ComponentModel.DataAnnotations;

namespace RealEstate.Application.DTOs
{
    /// <summary>
    /// Represents a data transfer object for building type details.
    /// Created By - Prashant
    /// Created Date - 14.11.2025
    /// </summary>
    public class BuildingTypeMasterDTO
    {
        public int SrNo { get; set; }
        public int? BuildingTypeIDP { get; set; }
        public int PropertyTypeIDF { get; set; }
        public string BuildingTypeName { get; set; }
        public string PropertyTypeName { get; set; }
        public bool IsActive { get; set; }
        public string CreatedDate { get; set; }
    }

    /// <summary>
    /// Represents a paging response for building type entities.
    /// Created By - Prashant
    /// Created Date - 14.11.2025
    /// </summary>
    public class BuildingTypeMasterPagingDTO
    {
        public List<BuildingTypeMasterDTO> Records { get; set; }
        public int TotalRecord { get; set; }
    }

    /// <summary>
    /// Represents a data transfer object for saving or updating building type details.
    /// Created By - Prashant
    /// Created Date - 14.11.2025
    /// </summary>
    public class BuildingTypeMasterSaveDTO
    {
        public int? BuildingTypeIDP { get; set; }

        [Required(ErrorMessage = "Please enter Building Type Name.")]
        [StringLength(150, ErrorMessage = "Building Type Name cannot exceed 150 characters.")]
        [MinLength(3, ErrorMessage = "Building Type Name must be at least 3 characters long.")]
        public string BuildingTypeName { get; set; }

        [Required(ErrorMessage = "Please select Property Type.")]
        public int PropertyTypeIDF { get; set; }

        public Guid UserIDF { get; set; }
    }

}
