using Microsoft.AspNetCore.Mvc;
using RealEstate.Application.DTOs;
using RealEstate.Application.DTOs.CommonDTOs;
using RealEstate.Application.Interface;
using RealEstate.Utilities.Constants;
using SQLHelper;

namespace RealEstate.Web.Controllers
{
    /// <summary>
    /// Represents a controller for managing tenant operations, including saving, retrieving, paging, and performing general actions on tenants.
    /// Created By - Nirmal
    /// Created Date - 13.11.2025
    /// </summary>
    public class TenantMasterController : BaseController
    {
        #region Variables
        private readonly ITenantMaster _tenantMaster;
        private readonly ILogger<TenantMasterController> _logger;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="TenantMasterController"/> class.
        /// Created By - Nirmal
        /// Created Date - 13.11.2025
        /// </summary>
        /// <param name="tenantMaster">The service for tenant master operations.</param>
        /// <param name="logger">The logger for logging information and errors.</param>
        public TenantMasterController(ITenantMaster tenantMaster, ILogger<TenantMasterController> logger)
        {
            _tenantMaster = tenantMaster;
            _logger = logger;
        }
        #endregion

        #region Index
        /// <summary>
        /// Displays the main tenant management view.
        /// Created By - Nirmal
        /// Created Date - 13.11.2025
        /// </summary>
        public IActionResult Index()
        {
            return View();
        }
        #endregion

        #region Save
        /// <summary>
        /// Saves or updates a tenant. If TenantIDP = 0, a new record is inserted, otherwise updated.
        /// Created By - Nirmal
        /// Created Date - 13.11.2025
        /// </summary>
        /// <param name="dto">The data transfer object containing tenant details to save or update.</param>
        /// <returns>A <see cref="JsonResult"/> indicating success or failure.</returns>
        [HttpPost]
        public async Task<IActionResult> Save(TenantMasterSaveDTO dto)
        {
            try
            {
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

                // Assign current user ID
                dto.UserIDF = GetCurrentUserId;
                var result = await _tenantMaster.SaveAsync(dto);

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
        /// Retrieves tenant details by ID.
        /// Created By - Nirmal
        /// Created Date - 13.11.2025
        /// </summary>
        /// <param name="id">The ID of the tenant to retrieve.</param>
        /// <returns>A <see cref="JsonResult"/> containing tenant data or an error message.</returns>
        [HttpPost]
        public async Task<IActionResult> GetByID(int id)
        {
            try
            {
                var data = await _tenantMaster.GetByIdAsync(id);
                return new JsonResult(new { success = true, data });
            }
            catch (Exception ex)
            {
                string controllerName = ControllerContext.RouteData.Values["controller"]?.ToString();
                string actionName = ControllerContext.RouteData.Values["action"]?.ToString();
                ERRORREPORTING.ErrorLog(ex, $"{controllerName}/{actionName}", "Nirmal Nanera", GetCurrentUserId);

                return new JsonResult(new { success = false, message = "Error retrieving tenant." });
            }
        }
        #endregion Get By ID

        #region Get Data With Paging
        /// <summary>
        /// Fetches a paginated list of tenants created by the current user with optional search and sorting.
        /// Created By - Nirmal
        /// Created Date - 13.11.2025
        /// </summary>
        /// <returns>A <see cref="JsonResult"/> in DataTables format containing paged tenant data.</returns>
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

                // Filter tenants by logged-in user
                var data = await _tenantMaster.GetPagedAsync(model, GetCurrentUserId);

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

        #region General Action
        /// <summary>
        /// Performs delete (1) or status change (2) on a tenant record.
        /// Created By - Nirmal
        /// Created Date - 13.11.2025
        /// </summary>
        /// <param name="ID">The ID of the tenant to perform the action on.</param>
        /// <param name="ActionType">The action type (Delete or Status Change).</param>
        /// <returns>A <see cref="JsonResult"/> indicating success or failure.</returns>
        [HttpPost]
        public async Task<IActionResult> GeneralAction(int ID, ActionType ActionType)
        {
            try
            {
                var result = await _tenantMaster.GeneralActionAsync(ID, ActionType);

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
        #endregion General Action
    }
}
