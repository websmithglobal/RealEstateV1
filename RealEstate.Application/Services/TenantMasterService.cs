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
    /// Represents a service for managing tenant master operations, 
    /// including pagination, retrieval, saving, and general actions.
    /// Created By - Nirmal
    /// Created Date - 13.11.2025
    /// </summary>
    public class TenantMasterService : ITenantMaster
    {
        #region Variables
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the TenantMasterService with the provided dependencies.
        /// </summary>
        public TenantMasterService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Saves or updates a tenant master entity asynchronously.
        /// </summary>
        public async Task<SQLReturnMessageNValue> SaveAsync(TenantMasterSaveDTO dto)
        {
            try
            {
                // Check duplicate email or phone
                bool emailExists = await _context.TenantMaster
                    .AnyAsync(t => t.EmailAddress == dto.EmailAddress &&
                                   (!dto.TenantIDP.HasValue || t.TenantIDP != dto.TenantIDP));

                if (emailExists)
                    return new SQLReturnMessageNValue { Outval = 99, Outmsg = OperationMessages.EmailExists };

                bool mobileExists = await _context.TenantMaster
                    .AnyAsync(t => t.ContactNo == dto.ContactNo &&
                                   (!dto.TenantIDP.HasValue || t.TenantIDP != dto.TenantIDP));

                if (mobileExists)
                    return new SQLReturnMessageNValue { Outval = 99, Outmsg = OperationMessages.MobileExists };

                if (dto.TenantIDP == null || dto.TenantIDP == 0)
                {
                    // INSERT
                    var entity = _mapper.Map<TenantMasterEntity>(dto);
                    entity.IsActive = true;
                    entity.IsDelete = false;
                    entity.CreatedDate = DateTime.UtcNow;

                    await _context.TenantMaster.AddAsync(entity);
                    await _context.SaveChangesAsync();

                    return new SQLReturnMessageNValue
                    {
                        Outval = 1,
                        Outmsg = OperationMessages.Save
                    };
                }
                else
                {
                    // UPDATE
                    var entity = await _context.TenantMaster.FirstOrDefaultAsync(t => t.TenantIDP == dto.TenantIDP);
                    if (entity == null)
                        return new SQLReturnMessageNValue { Outval = 0, Outmsg = "Tenant not found." };

                    _mapper.Map(dto, entity);
                    entity.UpdatedBy = dto.UserIDF;
                    entity.UpdatedDate = DateTime.UtcNow;

                    _context.TenantMaster.Update(entity);
                    await _context.SaveChangesAsync();

                    return new SQLReturnMessageNValue
                    {
                        Outval = 1,
                        Outmsg = OperationMessages.Update
                    };
                }
            }
            catch (Exception ex)
            {
                return new SQLReturnMessageNValue
                {
                    Outval = 0,
                    Outmsg = OperationMessages.Error + ex.Message
                };
            }
        }

        /// <summary>
        /// Retrieves a tenant master entity by its ID asynchronously.
        /// </summary>
        public async Task<TenantMasterDTO> GetByIdAsync(int TenantIDP)
        {
            try
            {
                var entity = await _context.TenantMaster.FirstOrDefaultAsync(x => x.TenantIDP == TenantIDP);
                if (entity == null)
                    return null;

                return _mapper.Map<TenantMasterDTO>(entity);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Retrieves paged tenant master entities asynchronously.
        /// </summary>
        public async Task<TenantMasterEntityPaging> GetPagedAsync(CommonPagingDTO paging, Guid LoginUserID)
        {
            try
            {
                // Get user and role
                var user = await _context.UserMaster
                    .FirstOrDefaultAsync(u => u.AspNetUserIDF == LoginUserID && !u.IsDelete);

                if (user == null)
                    return new TenantMasterEntityPaging
                    {
                        Records = new List<TenantMasterDTO>(),
                        TotalRecord = 0
                    };

                var role = await (from ur in _context.UserRoles
                                  join r in _context.Roles on ur.RoleId equals r.Id
                                  where ur.UserId == LoginUserID.ToString()
                                  select r.Name).FirstOrDefaultAsync();

                // Base query
                var query = _context.TenantMaster.Where(t => !t.IsDelete);

                // Role-based filter
                if (!(role.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                      role.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase)))
                {
                    query = query.Where(t => t.CreatedBy == LoginUserID);
                }

                // Search
                if (!string.IsNullOrWhiteSpace(paging.SearchValue))
                {
                    string search = paging.SearchValue.ToLower();
                    query = query.Where(t =>
                        t.TenantName.ToLower().Contains(search) ||
                        (t.EmailAddress != null && t.EmailAddress.ToLower().Contains(search)) ||
                        (t.ContactNo != null && t.ContactNo.ToLower().Contains(search)) ||
                        (t.Address != null && t.Address.ToLower().Contains(search))
                    );
                }

                // Count
                var totalCount = await query.CountAsync();

                // Ordering
                query = paging.OrderByFieldName?.ToLower() switch
                {
                    "tenantname" => paging.OrderByType == 1 ? query.OrderByDescending(t => t.TenantName) : query.OrderBy(t => t.TenantName),
                    "emailaddress" => paging.OrderByType == 1 ? query.OrderByDescending(t => t.EmailAddress) : query.OrderBy(t => t.EmailAddress),
                    "contactno" => paging.OrderByType == 1 ? query.OrderByDescending(t => t.ContactNo) : query.OrderBy(t => t.ContactNo),
                    "createddate" => paging.OrderByType == 1 ? query.OrderByDescending(t => t.CreatedDate) : query.OrderBy(t => t.CreatedDate),
                    _ => query.OrderByDescending(t => t.CreatedDate)
                };

                // Paging
                var tenants = await query
                    .Skip(paging.DisplayStart)
                    .Take(paging.DisplayLength)
                    .ToListAsync();

                // Map result
                int srNo = paging.DisplayStart + 1;
                var result = tenants.Select(t => new TenantMasterDTO
                {
                    SrNo = srNo++,
                    TenantIDP = t.TenantIDP ?? 0,
                    TenantName = t.TenantName,
                    ContactNo = t.ContactNo,
                    EmailAddress = t.EmailAddress,
                    Address = t.Address,
                    IsActive = t.IsActive,
                    CreatedDate = t.CreatedDate.HasValue ? t.CreatedDate.Value.ToString("dd-MM-yyyy") : string.Empty
                }).ToList();

                return new TenantMasterEntityPaging
                {
                    Records = result,
                    TotalRecord = totalCount
                };
            }
            catch (Exception)
            {
                return new TenantMasterEntityPaging
                {
                    Records = new List<TenantMasterDTO>(),
                    TotalRecord = 0
                };
            }
        }

        /// <summary>
        /// Performs a general action (delete or toggle active status) on a tenant asynchronously.
        /// </summary>
        public async Task<SQLReturnMessageNValue> GeneralActionAsync(int tenantId, ActionType actionType)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var entity = await _context.TenantMaster.FirstOrDefaultAsync(x => x.TenantIDP == tenantId);
                if (entity == null)
                    return new SQLReturnMessageNValue
                    {
                        Outval = 0,
                        Outmsg = OperationMessages.Error + "Tenant not found."
                    };

                switch (actionType)
                {
                    case ActionType.Delete:
                        entity.IsDelete = true;
                        entity.IsActive = false;
                        entity.UpdatedDate = DateTime.UtcNow;
                        _context.TenantMaster.Update(entity);
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        return new SQLReturnMessageNValue
                        {
                            Outval = 1,
                            Outmsg = OperationMessages.Delete
                        };

                    case ActionType.UpdateStatus:
                        entity.IsActive = !entity.IsActive;
                        entity.UpdatedDate = DateTime.UtcNow;
                        _context.TenantMaster.Update(entity);
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        return new SQLReturnMessageNValue
                        {
                            Outval = 1,
                            Outmsg = OperationMessages.StatusChange
                        };

                    default:
                        return new SQLReturnMessageNValue
                        {
                            Outval = 0,
                            Outmsg = OperationMessages.Error + "Invalid action type."
                        };
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new SQLReturnMessageNValue
                {
                    Outval = 0,
                    Outmsg = OperationMessages.Error + ex.Message
                };
            }
        }

        #endregion
    }
}