using Microsoft.AspNetCore.Mvc;
using RealEstate.Application.DTOs;
using RealEstate.Application.DTOs.CommonDTOs;
using RealEstate.Application.Interface;
using RealEstate.Utilities.Constants;
using SQLHelper;

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
                if (dto.UserIDP != 0)
                {
                    ModelState.Remove(nameof(dto.Password));
                    ModelState.Remove(nameof(dto.ConfirmPassword));
                }

                // Model validation
                if (!ModelState.IsValid)
                {
                    var fieldErrors = ModelState
                        .Where(x => x.Value?.Errors.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value.Errors.First().ErrorMessage
                        );

                    return new JsonResult(new
                    {
                        Outval = 0,
                        Outmsg = "Please correct the highlighted errors.",
                        FieldErrors = fieldErrors
                    });
                }

                dto.UserIDF = GetCurrentUserId;
                var result = await _userMaster.SaveAsync(dto);

                if (result.Outval.ToString() == "99")
                {
                    if (result.Outmsg.Contains("Email"))
                        result.Outmsg = OperationMessages.EmailExists;
                    else if (result.Outmsg.Contains("Mobile"))
                        result.Outmsg = OperationMessages.MobileExists;
                }

                return new JsonResult(new
                {
                    Outval = result.Outval,
                    Outmsg = result.Outmsg
                });
            }
            catch (Exception ex)
            {
                string controllerName = ControllerContext.RouteData.Values["controller"]?.ToString();
                string actionName = ControllerContext.RouteData.Values["action"]?.ToString();
                ERRORREPORTING.ErrorLog(ex, $"{controllerName}/{actionName}", "Nirmal Nanera", GetCurrentUserId);

                return new JsonResult(new
                {
                    Outval = 0,
                    Outmsg = OperationMessages.Error + ex.Message
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
                string controllerName = ControllerContext.RouteData.Values["controller"]?.ToString();
                string actionName = ControllerContext.RouteData.Values["action"]?.ToString();

                ERRORREPORTING.ErrorLog(ex, $"{controllerName}/{actionName}", "Nirmal Nanera", GetCurrentUserId);
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
                string controllerName = ControllerContext.RouteData.Values["controller"]?.ToString();
                string actionName = ControllerContext.RouteData.Values["action"]?.ToString();

                ERRORREPORTING.ErrorLog(ex, $"{controllerName}/{actionName}", "Nirmal Nanera", GetCurrentUserId);
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
        [HttpPost]
        public async Task<IActionResult> GeneralAction(int ID, ActionType ActionType)
        {
            try
            {
                var result = await _userMaster.GeneralActionAsync(ID, ActionType);

                return new JsonResult(new
                {
                    success = result.Outval,
                    message = result.Outmsg
                });
            }
            catch (Exception ex)
            {
                string controllerName = ControllerContext.RouteData.Values["controller"]?.ToString();
                string actionName = ControllerContext.RouteData.Values["action"]?.ToString();

                ERRORREPORTING.ErrorLog(ex, $"{controllerName}/{actionName}", "Nirmal Nanera", GetCurrentUserId);

                return new JsonResult(new
                {
                    success = false,
                    message = OperationMessages.Error + ex.Message
                });
            }
        }
        #endregion General Action
    }
}
