using RealEstate.Application.DTOs;
using RealEstate.Application.DTOs.CommonDTOs;
using RealEstate.Utilities.Constants;
using static RealEstate.Utilities.MEMBERS;

namespace RealEstate.Application.Interface
{
    /// <summary>
    /// Interface defining Property Type CRUD operations.
    /// Created By - Prashant
    /// Created Date - 14.11.2025
    /// </summary>
    public interface IPropertyTypeMasterService
    {
        Task<SQLReturnMessageNValue> SaveAsync(PropertyTypeMasterSaveDTO dto);
        Task<PropertyTypeMasterDTO> GetByIdAsync(int id);
        Task<PropertyTypeMasterPagingDTO> GetPagedAsync(CommonPagingDTO paging);
        Task<SQLReturnMessageNValue> GeneralActionAsync(int id, ActionType actionType);
        List<PropertyTypeMasterDTO> GetDropdownList();
    }
}
