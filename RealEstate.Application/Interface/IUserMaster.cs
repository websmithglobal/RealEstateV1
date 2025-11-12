using RealEstate.Application.DTOs;
using RealEstate.Application.DTOs.CommonDTOs;
using RealEstate.Utilities.Constants;
using static RealEstate.Utilities.MEMBERS;

namespace RealEstate.Application.Interface
{
    /// <summary>
    /// Represents an interface for user master operations, including pagination, retrieval, saving, and general actions.
    /// Created By - Nirmal
    /// Created Date - 12.11.2025
    /// </summary>
    public interface IUserMaster
    {
        /// <summary>
        /// Retrieves paged user master entities asynchronously based on paging parameters and the logged-in user ID.
        /// Created By - Nirmal
        /// Created Date - 12.11.2025
        /// </summary>
        /// <param name="paging">The paging and sorting parameters.</param>
        /// <param name="LoginUserID">The ID of the logged-in user.</param>
        /// <returns>A task representing the asynchronous operation, returning the paged user master entities.</returns>
        Task<UserMasterEntityPaging> GetPagedAsync(CommonPagingDTO paging, Guid LoginUserID);

        /// <summary>
        /// Retrieves a user master entity by its ID asynchronously.
        /// Created By - Nirmal
        /// Created Date - 12.11.2025
        /// </summary>
        /// <param name="userID">The ID of the user to retrieve.</param>
        /// <returns>A task representing the asynchronous operation, returning the user master details.</returns>
        Task<UserMasterDTO> GetByIdAsync(int userID);

        /// <summary>
        /// Saves or updates a user master entity asynchronously.
        /// Created By - Nirmal
        /// Created Date - 12.11.2025
        /// </summary>
        /// <param name="dto">The data transfer object containing user details to save.</param>
        /// <returns>A task representing the asynchronous operation, returning true if successful.</returns>
        Task<SQLReturnMessageNValue> SaveAsync(UserMasterSaveDTO dto);

        /// <summary>
        /// Performs a general action on a user asynchronously, such as activation or deactivation.
        /// Created By - Nirmal
        /// Created Date - 12.11.2025
        /// </summary>
        /// <param name="userId">The ID of the user to perform the action on.</param>
        /// <param name="actionType">The type of action to perform.</param>
        /// <returns>A task representing the asynchronous operation, returning true if successful.</returns>
        Task<bool> GeneralActionAsync(int userId, ActionType actionType);
    }
}
