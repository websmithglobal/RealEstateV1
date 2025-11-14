using RealEstate.Application.DTOs;
using RealEstate.Application.DTOs.CommonDTOs;
using RealEstate.Utilities.Constants;
using static RealEstate.Utilities.MEMBERS;

namespace RealEstate.Application.Interface
{
    public interface IBuildingTypeMasterService
    {
        Task<SQLReturnMessageNValue> SaveAsync(BuildingTypeMasterSaveDTO dto);
        Task<BuildingTypeMasterDTO> GetByIdAsync(int id);
        Task<BuildingTypeMasterPagingDTO> GetPagedAsync(CommonPagingDTO paging);
        Task<SQLReturnMessageNValue> GeneralActionAsync(int id, ActionType actionType);
    }
}
