using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RealEstate.Application.DTOs;
using RealEstate.Application.DTOs.CommonDTOs;
using RealEstate.Application.Interface;
using RealEstate.Core.Data;
using RealEstate.Core.Data.Entities;
using RealEstate.Utilities.Constants;
using static RealEstate.Utilities.MEMBERS;

namespace RealEstate.Application.Services
{
    /// <summary>
    /// Service class for managing Property Type master data.
    /// Created By - Prashant
    /// Created Date - 14.11.2025
    /// </summary>
    public class PropertyTypeMasterService : IPropertyTypeMasterService
    {

        #region Variables
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        #endregion

        #region Constructor

        public PropertyTypeMasterService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        #endregion

        #region SaveAsync

        public async Task<SQLReturnMessageNValue> SaveAsync(PropertyTypeMasterSaveDTO dto)
        {
            try
            {
                bool exists = await _context.PropertyTypeMaster
                    .AnyAsync(x => x.PropertyTypeName.ToLower() == dto.PropertyTypeName.ToLower()
                                   && (x.PropertyTypeIDP != dto.PropertyTypeIDP));

                if (exists)
                    return new SQLReturnMessageNValue { Outval = 99, Outmsg = "Property Type already exists." };

                if (dto.PropertyTypeIDP == null || dto.PropertyTypeIDP == 0)
                {
                    var entity = _mapper.Map<PropertyTypeMasterEntity>(dto);
                    entity.IsActive = true;
                    entity.IsDelete = false;
                    entity.CreatedDate = DateTime.UtcNow;

                    await _context.PropertyTypeMaster.AddAsync(entity);
                    await _context.SaveChangesAsync();

                    return new SQLReturnMessageNValue { Outval = 1, Outmsg = OperationMessages.Save };
                }
                else
                {
                    var entity = await _context.PropertyTypeMaster
                        .FirstOrDefaultAsync(x => x.PropertyTypeIDP == dto.PropertyTypeIDP);

                    if (entity == null)
                        return new SQLReturnMessageNValue { Outval = 0, Outmsg = "Property Type not found." };

                    _mapper.Map(dto, entity);
                    entity.UpdatedBy = dto.UserIDF;
                    entity.UpdatedDate = DateTime.UtcNow;

                    _context.PropertyTypeMaster.Update(entity);
                    await _context.SaveChangesAsync();

                    return new SQLReturnMessageNValue { Outval = 1, Outmsg = OperationMessages.Update };
                }
            }
            catch (Exception ex)
            {
                return new SQLReturnMessageNValue { Outval = 0, Outmsg = "Error: " + ex.Message };
            }
        }

        #endregion

        #region GetByIdAsync

        public async Task<PropertyTypeMasterDTO> GetByIdAsync(int id)
        {
            try
            {
                var entity = await _context.PropertyTypeMaster
                    .FirstOrDefaultAsync(x => x.PropertyTypeIDP == id && !x.IsDelete);

                return entity == null ? null : _mapper.Map<PropertyTypeMasterDTO>(entity);
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region GetPagedAsync

        public async Task<PropertyTypeMasterPagingDTO> GetPagedAsync(CommonPagingDTO paging)
        {
            try
            {
                var query = _context.PropertyTypeMaster.Where(x => !x.IsDelete);

                if (!string.IsNullOrWhiteSpace(paging.SearchValue))
                {
                    string search = paging.SearchValue.ToLower();

                    query = query.Where(x =>
                        x.PropertyTypeName.ToLower().Contains(search));
                }

                int totalCount = await query.CountAsync();

                query = paging.OrderByFieldName?.ToLower() switch
                {
                    "propertytypename" =>
                        paging.OrderByType == 1 ? query.OrderByDescending(x => x.PropertyTypeName) : query.OrderBy(x => x.PropertyTypeName),

                    _ => query.OrderByDescending(x => x.CreatedDate)
                };

                var list = await query
                    .Skip(paging.DisplayStart)
                    .Take(paging.DisplayLength)
                    .ToListAsync();

                int sr = paging.DisplayStart + 1;

                var mappedList = list.Select(x => new PropertyTypeMasterDTO
                {
                    SrNo = sr++,
                    PropertyTypeIDP = x.PropertyTypeIDP ?? 0,
                    PropertyTypeName = x.PropertyTypeName,
                    IsActive = x.IsActive,
                    CreatedDate = x.CreatedDate?.ToString("dd-MM-yyyy")
                }).ToList();

                return new PropertyTypeMasterPagingDTO
                {
                    Records = mappedList,
                    TotalRecord = totalCount
                };
            }
            catch
            {
                return new PropertyTypeMasterPagingDTO
                {
                    Records = new List<PropertyTypeMasterDTO>(),
                    TotalRecord = 0
                };
            }
        }

        #endregion

        #region GeneralActionAsync

        public async Task<SQLReturnMessageNValue> GeneralActionAsync(int id, ActionType actionType)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var entity = await _context.PropertyTypeMaster.FirstOrDefaultAsync(x => x.PropertyTypeIDP == id);

                if (entity == null)
                    return new SQLReturnMessageNValue { Outval = 0, Outmsg = "Property Type not found." };

                switch (actionType)
                {
                    case ActionType.Delete:
                        entity.IsDelete = true;
                        entity.IsActive = false;
                        break;

                    case ActionType.UpdateStatus:
                        entity.IsActive = !entity.IsActive;
                        break;
                }

                entity.UpdatedDate = DateTime.UtcNow;

                _context.PropertyTypeMaster.Update(entity);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new SQLReturnMessageNValue
                {
                    Outval = 1,
                    Outmsg = actionType == ActionType.Delete ? OperationMessages.Delete : OperationMessages.StatusChange
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new SQLReturnMessageNValue { Outval = 0, Outmsg = "Error: " + ex.Message };
            }
        }

        #endregion

        #region Dropdown

        /// <summary>
        /// Returns active, non-deleted Property Types as dropdown list.
        /// Created By - Prashant
        /// Created Date - 14.11.2025
        /// </summary>
        public List<PropertyTypeMasterDTO> GetDropdownList()
        {
            try
            {
                var data = _context.PropertyTypeMaster
                    .Where(x => !x.IsDelete && x.IsActive)
                    .OrderBy(x => x.PropertyTypeName)
                    .ToList();

                return _mapper.Map<List<PropertyTypeMasterDTO>>(data);
            }
            catch
            {
                return new List<PropertyTypeMasterDTO>();
            }
        }
        #endregion
    }
}
