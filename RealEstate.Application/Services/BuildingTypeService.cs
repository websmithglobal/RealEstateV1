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
    /// Service class for managing Building Type master data.
    /// Created By - Nirmal
    /// Created Date - 14.11.2025
    /// </summary>
    public class BuildingTypeMasterService : IBuildingTypeMasterService
    {
        #region Variables
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        #endregion

        #region Constructor

        /// <summary>
        /// Initializes BuildingTypeMasterervice with dependencies.
        /// </summary>
        public BuildingTypeMasterService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        #endregion

        #region SaveAsync (Insert/Update)

        /// <summary>
        /// Saves or updates a Building Type record.
        /// </summary>
        public async Task<SQLReturnMessageNValue> SaveAsync(BuildingTypeMasterSaveDTO dto)
        {
            try
            {
                bool exists = await _context.BuildingTypeMaster
                    .AnyAsync(x => x.BuildingTypeName.ToLower() == dto.BuildingTypeName.ToLower()
                                   && (!dto.BuildingTypeIDP.HasValue || x.BuildingTypeIDP != dto.BuildingTypeIDP));

                if (exists)
                    return new SQLReturnMessageNValue { Outval = 99, Outmsg = "Building Type already exists." };

                if (dto.BuildingTypeIDP == null || dto.BuildingTypeIDP == 0)
                {
                    // INSERT
                    var entity = _mapper.Map<BuildingTypeMasterEntity>(dto);
                    entity.IsActive = true;
                    entity.IsDelete = false;
                    entity.CreatedDate = DateTime.UtcNow;

                    await _context.BuildingTypeMaster.AddAsync(entity);
                    await _context.SaveChangesAsync();

                    return new SQLReturnMessageNValue { Outval = 1, Outmsg = OperationMessages.Save };
                }
                else
                {
                    // UPDATE
                    var entity = await _context.BuildingTypeMaster
                        .FirstOrDefaultAsync(x => x.BuildingTypeIDP == dto.BuildingTypeIDP);

                    if (entity == null)
                        return new SQLReturnMessageNValue { Outval = 0, Outmsg = "Building Type not found." };

                    _mapper.Map(dto, entity);
                    entity.UpdatedBy = dto.UserIDF;
                    entity.UpdatedDate = DateTime.UtcNow;

                    _context.BuildingTypeMaster.Update(entity);
                    await _context.SaveChangesAsync();

                    return new SQLReturnMessageNValue { Outval = 1, Outmsg = OperationMessages.Update };
                }
            }
            catch (Exception ex)
            {
                return new SQLReturnMessageNValue { Outval = 0, Outmsg = OperationMessages.Error + ex.Message };
            }
        }

        #endregion

        #region GetByIdAsync

        /// <summary>
        /// Retrieves a building type by ID.
        /// </summary>
        public async Task<BuildingTypeMasterDTO> GetByIdAsync(int id)
        {
            try
            {
                var entity = await _context.BuildingTypeMaster
                    .Include(x => x.PropertyTypeMaster)
                    .FirstOrDefaultAsync(x => x.BuildingTypeIDP == id && !x.IsDelete);

                return entity == null ? null : _mapper.Map<BuildingTypeMasterDTO>(entity);
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region GetPagedAsync

        /// <summary>
        /// Retrieves paginated building type list.
        /// </summary>
        public async Task<BuildingTypeMasterPagingDTO> GetPagedAsync(CommonPagingDTO paging)
        {
            try
            {
                var query = _context.BuildingTypeMaster
                    .Include(x => x.PropertyTypeMaster)
                    .Where(x => !x.IsDelete);

                // Search
                if (!string.IsNullOrWhiteSpace(paging.SearchValue))
                {
                    string search = paging.SearchValue.ToLower();

                    query = query.Where(x =>
                        x.BuildingTypeName.ToLower().Contains(search) ||
                        x.PropertyTypeMaster.PropertyTypeName.ToLower().Contains(search));
                }

                int totalCount = await query.CountAsync();

                query = paging.OrderByFieldName?.ToLower() switch
                {
                    "buildingtypename" =>
                        paging.OrderByType == 1 ? query.OrderByDescending(x => x.BuildingTypeName) : query.OrderBy(x => x.BuildingTypeName),

                    _ => query.OrderByDescending(x => x.CreatedDate)
                };

                var list = await query
                    .Skip(paging.DisplayStart)
                    .Take(paging.DisplayLength)
                    .ToListAsync();

                int sr = paging.DisplayStart + 1;

                var mappedList = list.Select(x => new BuildingTypeMasterDTO
                {
                    SrNo = sr++,
                    BuildingTypeIDP = x.BuildingTypeIDP ?? 0,
                    BuildingTypeName = x.BuildingTypeName,
                    IsActive = x.IsActive,
                    PropertyTypeName = x.PropertyTypeMaster.PropertyTypeName,
                    CreatedDate = x.CreatedDate?.ToString("dd-MM-yyyy")
                }).ToList();

                return new BuildingTypeMasterPagingDTO
                {
                    Records = mappedList,
                    TotalRecord = totalCount
                };
            }
            catch
            {
                return new BuildingTypeMasterPagingDTO
                {
                    Records = new List<BuildingTypeMasterDTO>(),
                    TotalRecord = 0
                };
            }
        }

        #endregion

        #region GeneralActionAsync

        /// <summary>
        /// Performs delete or toggle status action for Building Type.
        /// </summary>
        public async Task<SQLReturnMessageNValue> GeneralActionAsync(int id, ActionType actionType)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var entity = await _context.BuildingTypeMaster.FirstOrDefaultAsync(x => x.BuildingTypeIDP == id);

                if (entity == null)
                    return new SQLReturnMessageNValue { Outval = 0, Outmsg = "Building Type not found." };

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

                _context.BuildingTypeMaster.Update(entity);
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
    }
}
