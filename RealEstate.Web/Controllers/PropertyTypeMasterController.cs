using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RealEstate.Application.DTOs;
using RealEstate.Application.DTOs.CommonDTOs;
using RealEstate.Application.Interface;
using RealEstate.Utilities.Constants;
using SQLHelper;

namespace RealEstate.Web.Controllers
{
    /// <summary>
    /// Represents a controller for managing property type master operations,
    /// including saving, retrieving, paging, and performing general actions.
    /// Created By - Prashant
    /// Created Date - 14.11.2025
    /// </summary>
    public class PropertyTypeMasterController : BaseController
    {
        #region Variables
        private readonly IPropertyTypeMasterService _propertyTypeMasterService;
        private readonly IBuildingTypeMasterService _buildingTypeMasterService;
        private readonly ILogger<PropertyTypeMasterController> _logger;
        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyTypeMasterController"/> class.
        /// Created By - Prashant
        /// Created Date - 14.11.2025
        /// </summary>
        public PropertyTypeMasterController(IPropertyTypeMasterService propertyTypeService,
                                            IBuildingTypeMasterService buildingTypeMasterService,
                                            ILogger<PropertyTypeMasterController> logger)
        {
            _propertyTypeMasterService = propertyTypeService;
            _buildingTypeMasterService = buildingTypeMasterService;
            _logger = logger;
        }

        #endregion

        #region Helper – Dynamic Controller/Action

        /// <summary>
        /// Returns "ControllerName/ActionName" using the current route data.
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
        /// Displays the Property Type Master view.
        /// Created By - Prashant
        /// Created Date - 14.11.2025
        /// </summary>
        public IActionResult Index()
        {
            return View();
        }

        #endregion

        #region Save

        /// <summary>
        /// Saves or updates a property type record.
        /// Created By - Prashant
        /// Created Date - 14.11.2025
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Save(PropertyTypeMasterSaveDTO dto)
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
                var result = await _propertyTypeMasterService.SaveAsync(dto);

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

        #region GetByID

        /// <summary>
        /// Retrieves property type details by ID.
        /// Created By - Prashant
        /// Created Date - 14.11.2025
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> GetByID(int id)
        {
            try
            {
                PropertyTypeMasterDTO data = await _propertyTypeMasterService.GetByIdAsync(id) ?? new PropertyTypeMasterDTO();

                return new JsonResult(new { success = true, data });
            }
            catch (Exception ex)
            {
                ERRORREPORTING.ErrorLog(ex, GetCurrentRoute(), "Prashant", GetCurrentUserId);

                return new JsonResult(new { success = false, message = "Error retrieving property type." });
            }
        }

        #endregion

        #region GetDataWithPaging

        /// <summary>
        /// Retrieves paginated property type data.
        /// Created By - Prashant
        /// Created Date - 14.11.2025
        /// </summary>
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

                var data = await _propertyTypeMasterService.GetPagedAsync(model);

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
                    data = new List<PropertyTypeMasterDTO>(),
                    recordsTotal = 0,
                    recordsFiltered = 0
                });
            }
        }

        #endregion

        #region GeneralAction

        /// <summary>
        /// Performs delete or status toggle action on a property type.
        /// Created By - Prashant
        /// Created Date - 14.11.2025
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> GeneralAction(int ID, ActionType ActionType)
        {
            try
            {
                var result = await _propertyTypeMasterService.GeneralActionAsync(ID, ActionType);

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
