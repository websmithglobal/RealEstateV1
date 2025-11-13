using RealEstate.Application.DTOs;
using RealEstate.Application.DTOs.CommonDTOs;
using RealEstate.Utilities.Constants;
using static RealEstate.Utilities.MEMBERS;

namespace RealEstate.Application.Interface
{
    public interface ITenantMaster
    {
        Task<TenantMasterEntityPaging> GetPagedAsync(CommonPagingDTO paging, Guid LoginUserID);
        Task<TenantMasterDTO> GetByIdAsync(int TenantIDP);
        Task<SQLReturnMessageNValue> SaveAsync(TenantMasterSaveDTO dto);
        Task<SQLReturnMessageNValue> GeneralActionAsync(int userId, ActionType actionType);
    }
}
