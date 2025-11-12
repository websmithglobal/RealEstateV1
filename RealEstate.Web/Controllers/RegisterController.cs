using Microsoft.AspNetCore.Mvc;
using RealEstate.Application.DTOs;
using RealEstate.Application.DTOs.CommonDTOs;
using RealEstate.Application.Interface;
using RealEstate.Utilities.Constants;

namespace RealEstate.Web.Controllers
{
    /// <summary>
    /// Represents a controller for handling user registration and management operations, including saving, retrieving, paging, and performing general actions on users.
    /// Created By - Nirmal
    /// Created Date - 12.11.2025
    /// </summary>
    public class RegisterController : BaseController
    {
        #region Variables
        private readonly IUserMaster _userMaster;
        private readonly ILogger<RegisterController> _logger;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterController"/> class.
        /// Created By - Nirmal
        /// Created Date - 12.11.2025
        /// </summary>
        /// <param name="userMaster">The service for user master operations.</param>
        /// <param name="logger">The logger for logging information and errors.</param>
        public RegisterController(IUserMaster userMaster, ILogger<RegisterController> logger)
        {
            _userMaster = userMaster;
            _logger = logger;
        }
        #endregion

        #region Save 
        /// <summary>
        /// Saves or updates a user. If UserIDP = 0, a new record is inserted, otherwise updated.
        /// Created By - Nirmal
        /// Created Date - 12.11.2025
        /// </summary>
        /// <param name="dto">The data transfer object containing user details to save or update.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="JsonResult"/> indicating success or failure.</returns>
        [HttpPost]
        public async Task<IActionResult> Save(UserMasterSaveDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new JsonResult(new
                    {
                        success = false,
                        message = "Invalid input data."
                    });
                }

                dto.UserIDF = GetCurrentUserId;

                var result = await _userMaster.SaveAsync(dto);

                bool success = result.Outval != null && result.Outval.ToString() == "1";

                return new JsonResult(new
                {
                    success = success,
                    message = result.Outmsg,
                    code = result.Outval // optional: return 1, 0, 99 for frontend logic
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Register/Save");
                return new JsonResult(new
                {
                    success = false,
                    message = $"Error while saving user: {ex.Message}"
                });
            }
        }
        #endregion Save

        #region Get By ID
        /// <summary>
        /// Retrieves user details by ID.
        /// Created By - Nirmal
        /// Created Date - 12.11.2025
        /// </summary>
        /// <param name="id">The ID of the user to retrieve.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="JsonResult"/> with user data or error message.</returns>
        [HttpPost]
        public async Task<IActionResult> GetByID(int id)
        {
            try
            {
                var data = await _userMaster.GetByIdAsync(id);
                return new JsonResult(new { success = true, data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Register/GetByID");
                return new JsonResult(new { success = false, message = "Error retrieving user." });
            }
        }
        #endregion Get By ID

        #region Get Data With Paging
        /// <summary>
        /// Fetches paged user list with search and sorting.
        /// Created By - Nirmal
        /// Created Date - 12.11.2025
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="JsonResult"/> in DataTables format with paged data or error handling.</returns>
        [HttpPost]
        public async Task<IActionResult> GetDataWithPaging()
        {
            try
            {
                var form = Request.Form;
                string draw = form["draw"];
                int.TryParse(form["length"], out int pageSize);
                int.TryParse(form["start"], out int skip);
                int.TryParse(form["order[0][column]"], out int sortColumnIndex);
                string sortColumnData = form[$"columns[{sortColumnIndex}][data]"];
                string sortColumnName = !string.IsNullOrEmpty(sortColumnData)
                    ? sortColumnData
                    : "CreatedDate";
                string sortDirStr = form["order[0][dir]"];
                int sortDir = sortDirStr == "asc" ? 0 : 1;
                string searchValue = form["search[value]"];
                CommonPagingDTO model = new()
                {
                    DisplayLength = pageSize,
                    DisplayStart = skip,
                    OrderByFieldName = sortColumnName,
                    OrderByType = sortDir,
                    SearchValue = searchValue
                };
                var data = await _userMaster.GetPagedAsync(model, GetCurrentUserId);
                return Json(new
                {
                    draw,
                    data = data.Records,
                    recordsTotal = data.TotalRecord,
                    recordsFiltered = data.TotalRecord
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Register/GetDataWithPaging");
                return Json(new
                {
                    draw = "",
                    data = new List<object>(),
                    recordsTotal = 0,
                    recordsFiltered = 0
                });
            }
        }
        #endregion Get Data With Paging

        #region General Action (Delete / Status Change)
        /// <summary>
        /// Performs delete (1) or status change (2) on a user.
        /// Created By - Nirmal
        /// Created Date - 12.11.2025
        /// </summary>
        /// <param name="ID">The ID of the user to perform the action on.</param>
        /// <param name="ActionType">The type of action to perform (Delete or UpdateStatus).</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="JsonResult"/> indicating success or failure with appropriate message.</returns>
        [HttpPost]
        public async Task<IActionResult> GeneralAction(int ID, ActionType ActionType)
        {
            try
            {
                bool result = await _userMaster.GeneralActionAsync(ID, ActionType);
                return new JsonResult(new
                {
                    success = result,
                    message = result
                         ? (ActionType == ActionType.Delete
                    ? "User deleted successfully."
                    : "User status updated successfully.")
                : "Action failed."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Register/GeneralAction");
                return new JsonResult(new
                {
                    success = false,
                    message = $"Error performing action: {ex.Message}"
                });
            }
        }
        #endregion General Action
    }
}
