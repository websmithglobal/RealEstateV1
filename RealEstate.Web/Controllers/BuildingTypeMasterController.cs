using Microsoft.AspNetCore.Mvc;
using RealEstate.Application.DTOs;
using RealEstate.Application.DTOs.CommonDTOs;
using RealEstate.Application.Interface;
using RealEstate.Utilities.Constants;
using SQLHelper;

namespace RealEstate.Web.Controllers
{
    /// <summary>
    /// Controller for managing building type operations,
    /// including save, update, paging, and general actions.
    /// Created By - Prashant
    /// Created Date - 14.11.2025
    /// </summary>
    public class BuildingTypeMasterController : BaseController
    {
        #region Variables
        private readonly IBuildingTypeMasterService _buildingTypeService;
        private readonly IPropertyTypeMasterService _propertyTypeMasterService;
        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="BuildingTypeMasterController"/> class.
        /// Created By - Prashant
        /// Created Date - 14.11.2025
        /// </summary>
        public BuildingTypeMasterController(IBuildingTypeMasterService buildingTypeService, IPropertyTypeMasterService propertyTypeMasterService)
        {
            _buildingTypeService = buildingTypeService;
            _propertyTypeMasterService = propertyTypeMasterService;
        }

        #endregion

        #region Helper – Dynamic Controller/Action

        /// <summary>
        /// Returns "ControllerName/ActionName" using the current route data.
        /// Created By - Prashant
        /// Created Date - 14.11.2025
        /// </summary>
        private string GetCurrentRoute()
        {
            var controller = ControllerContext.RouteData.Values["controller"]?.ToString() ?? "Unknown";
            var action = ControllerContext.RouteData.Values["action"]?.ToString() ?? "Unknown";
            return $"{controller}/{action}";
        }

        #endregion

        #region Index

        /// <summary>
        /// Displays the Building Type Master index view.
        /// Created By - Prashant
        /// Created Date - 14.11.2025
        /// </summary>
        /// <returns>The index view.</returns>
        public IActionResult Index()
        {
            #region Dropdown Bindings
            List<PropertyTypeMasterDTO> propertyTypeMasterDTO = _propertyTypeMasterService.GetDropdownList();
            ViewBag.PropertyTypeDropdownList = propertyTypeMasterDTO;
            #endregion
            return View();
        }

        #endregion

        #region Save

        /// <summary>
        /// Saves or updates a building type record.
        /// Created By - Prashant
        /// Created Date - 14.11.2025
        /// </summary>
        /// <param name="dto">The building type data transfer object containing the record details.</param>
        /// <returns>JSON result indicating success/failure with output message and field errors if any.</returns>
        [HttpPost]
        public async Task<IActionResult> Save(BuildingTypeMasterSaveDTO dto)
        {
            try
            {
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
                var result = await _buildingTypeService.SaveAsync(dto);

                return new JsonResult(new
                {
                    Outval = result.Outval,
                    Outmsg = result.Outmsg
                });
            }
            catch (Exception ex)
            {
                ERRORREPORTING.ErrorLog(ex, GetCurrentRoute(), "Prashant", GetCurrentUserId);

                return new JsonResult(new
                {
                    Outval = 0,
                    Outmsg = OperationMessages.Error + ex.Message
                });
            }
        }

        #endregion

        #region GetById

        /// <summary>
        /// Retrieves a building type record by its ID.
        /// Created By - Prashant
        /// Created Date - 14.11.2025
        /// </summary>
        /// <param name="id">The unique identifier of the building type.</param>
        /// <returns>JSON result with success flag and data or error message.</returns>
        [HttpPost]
        public async Task<IActionResult> GetByID(int id)
        {
            try
            {
                BuildingTypeMasterDTO data = await _buildingTypeService.GetByIdAsync(id) ?? new BuildingTypeMasterDTO();
                return new JsonResult(new { success = true, data });
            }
            catch (Exception ex)
            {
                ERRORREPORTING.ErrorLog(ex, GetCurrentRoute(), "Prashant", GetCurrentUserId);

                return new JsonResult(new { success = false, message = "Error retrieving building type." });
            }
        }

        #endregion

        #region GetDataWithPaging

        /// <summary>
        /// Retrieves paginated list of building types based on DataTables parameters.
        /// Created By - Prashant
        /// Created Date - 14.11.2025
        /// </summary>
        /// <returns>JSON result compatible with DataTables (draw, recordsTotal, recordsFiltered, data).</returns>
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
                string sortColumnName = !string.IsNullOrEmpty(sortColumnData) ? sortColumnData : "CreatedDate";

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

                var data = await _buildingTypeService.GetPagedAsync(model);

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
                ERRORREPORTING.ErrorLog(ex, GetCurrentRoute(), "Prashant", GetCurrentUserId);

                return Json(new
                {
                    draw = "",
                    data = new List<object>(),
                    recordsTotal = 0,
                    recordsFiltered = 0
                });
            }
        }

        #endregion

        #region GeneralAction

        /// <summary>
        /// Performs general actions (delete or status toggle) on a building type record.
        /// Created By - Prashant
        /// Created Date - 14.11.2025
        /// </summary>
        /// <param name="ID">The ID of the building type record.</param>
        /// <param name="ActionType">The type of action to perform (Delete, Activate, Deactivate).</param>
        /// <returns>JSON result with success flag and message.</returns>
        [HttpPost]
        public async Task<IActionResult> GeneralAction(int ID, ActionType ActionType)
        {
            try
            {
                var result = await _buildingTypeService.GeneralActionAsync(ID, ActionType);
                return new JsonResult(new
                {
                    Outval = result.Outval,
                    Outmsg = result.Outmsg
                });
            }
            catch (Exception ex)
            {
                ERRORREPORTING.ErrorLog(ex, GetCurrentRoute(), "Prashant", GetCurrentUserId);

                return new JsonResult(new
                {
                    Outval = false,
                    Outmsg = OperationMessages.Error + ex.Message
                });
            }
        }

        #endregion
    }
}