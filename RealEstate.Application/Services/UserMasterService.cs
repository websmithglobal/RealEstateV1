using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RealEstate.Application.DTOs;
using RealEstate.Application.DTOs.CommonDTOs;
using RealEstate.Application.Interface;
using RealEstate.Core.Data;
using RealEstate.Core.Identity;
using RealEstate.Infrastructure.Data.Entities;
using RealEstate.Utilities.Constants;
using static RealEstate.Utilities.MEMBERS;

namespace RealEstate.Application.Services
{
    /// <summary>
    /// Represents a service for managing user master operations, including pagination, retrieval, saving, and general actions.
    /// Created By - Nirmal
    /// Created Date - 12.11.2025
    /// </summary>
    public class UserMasterService : IUserMaster
    {
        #region Variables
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the UserMasterService with the provided dependencies.
        /// Created By - Nirmal
        /// Created Date - 12.11.2025
        /// </summary>
        /// <param name="context">The application database context.</param>
        /// <param name="mapper">The AutoMapper instance for object mapping.</param>
        /// <param name="userManager">The user manager for Identity operations.</param>
        /// <param name="roleManager">The role manager for Identity role operations.</param>
        public UserMasterService(ApplicationDbContext context, IMapper mapper, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _mapper = mapper;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Saves or updates a user master entity asynchronously, handling both insert and update modes with Identity user management.
        /// Created By - Nirmal
        /// Created Date - 12.11.2025
        /// </summary>
        public async Task<SQLReturnMessageNValue> SaveAsync(UserMasterSaveDTO dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                string currentAspNetUserId = null;

                if (dto.UserIDP != null && dto.UserIDP != 0)
                {
                    var currentEntity = await _context.UserMaster
                        .AsNoTracking()
                        .FirstOrDefaultAsync(u => u.UserIDP == dto.UserIDP);

                    if (currentEntity != null)
                        currentAspNetUserId = currentEntity.AspNetUserIDF.ToString();
                }

                var emailExists = await _context.Users
                    .AnyAsync(u => u.Email == dto.Email && (currentAspNetUserId == null || u.Id != currentAspNetUserId));
                if (emailExists)
                    return new SQLReturnMessageNValue { Outval = 99, Outmsg = OperationMessages.EmailExists };

                var mobileExists = await _context.Users
                    .AnyAsync(u => u.PhoneNumber == dto.PhoneNumber && (currentAspNetUserId == null || u.Id != currentAspNetUserId));
                if (mobileExists)
                    return new SQLReturnMessageNValue { Outval = 99, Outmsg = OperationMessages.MobileExists };

                if (dto.UserIDP == null || dto.UserIDP == 0)
                {
                    var applicationUser = _mapper.Map<ApplicationUser>(dto);
                    var createResult = await _userManager.CreateAsync(applicationUser, dto.Password ?? "Default@123");
                    if (!createResult.Succeeded)
                        return new SQLReturnMessageNValue
                        {
                            Outval = 0,
                            Outmsg = OperationMessages.Error + string.Join("; ", createResult.Errors.Select(e => e.Description))
                        };

                    if (!await _roleManager.RoleExistsAsync(dto.Role))
                    {
                        var roleResult = await _roleManager.CreateAsync(new IdentityRole(dto.Role));
                        if (!roleResult.Succeeded)
                            return new SQLReturnMessageNValue
                            {
                                Outval = 0,
                                Outmsg = OperationMessages.Error + string.Join("; ", roleResult.Errors.Select(e => e.Description))
                            };
                    }

                    var addRoleResult = await _userManager.AddToRoleAsync(applicationUser, dto.Role);
                    if (!addRoleResult.Succeeded)
                        return new SQLReturnMessageNValue
                        {
                            Outval = 0,
                            Outmsg = OperationMessages.Error + string.Join("; ", addRoleResult.Errors.Select(e => e.Description))
                        };

                    var entity = _mapper.Map<UserMasterEntity>(dto);
                    _mapper.Map(applicationUser, entity);
                    entity.CreatedBy = dto.UserIDF;

                    _context.UserMaster.Add(entity);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    return new SQLReturnMessageNValue
                    {
                        Outval = 1,
                        Outmsg = OperationMessages.Save
                    };
                }
                else
                {
                    var entity = await _context.UserMaster.FirstOrDefaultAsync(u => u.UserIDP == dto.UserIDP);
                    if (entity == null)
                        return new SQLReturnMessageNValue { Outval = 0, Outmsg = "User not found in UserMaster." };

                    var aspUser = await _userManager.FindByIdAsync(entity.AspNetUserIDF.ToString());
                    if (aspUser == null)
                        return new SQLReturnMessageNValue { Outval = 0, Outmsg = "Associated AspNetUser not found." };

                    _mapper.Map(dto, aspUser);
                    var updateResult = await _userManager.UpdateAsync(aspUser);
                    if (!updateResult.Succeeded)
                        return new SQLReturnMessageNValue
                        {
                            Outval = 0,
                            Outmsg = OperationMessages.Error + string.Join("; ", updateResult.Errors.Select(e => e.Description))
                        };

                    if (!string.IsNullOrEmpty(dto.Password))
                    {
                        var token = await _userManager.GeneratePasswordResetTokenAsync(aspUser);
                        var resetResult = await _userManager.ResetPasswordAsync(aspUser, token, dto.Password);
                        if (!resetResult.Succeeded)
                            return new SQLReturnMessageNValue
                            {
                                Outval = 0,
                                Outmsg = OperationMessages.Error + string.Join("; ", resetResult.Errors.Select(e => e.Description))
                            };
                    }

                    var currentRoles = await _userManager.GetRolesAsync(aspUser);
                    if (!currentRoles.Contains(dto.Role))
                    {
                        await _userManager.RemoveFromRolesAsync(aspUser, currentRoles);
                        await _userManager.AddToRoleAsync(aspUser, dto.Role);
                    }

                    _mapper.Map(dto, entity);
                    entity.UpdatedBy = dto.UserIDF;
                    entity.UpdatedDate = DateTime.UtcNow;

                    _context.UserMaster.Update(entity);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    return new SQLReturnMessageNValue
                    {
                        Outval = 1,
                        Outmsg = OperationMessages.Update
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

        /// <summary>
        /// Retrieves a user master entity by its ID asynchronously.
        /// </summary>
        public async Task<UserMasterDTO> GetByIdAsync(int userID)
        {
            try
            {
                var entity = await _context.UserMaster.FirstOrDefaultAsync(x => x.UserIDP == userID);
                if (entity == null)
                    throw new Exception("User not found.");

                var aspUser = await _userManager.FindByIdAsync(entity.AspNetUserIDF.ToString());
                if (aspUser == null)
                    throw new Exception("Associated AspNetUser not found.");

                var roles = await _userManager.GetRolesAsync(aspUser);
                string role = roles.FirstOrDefault() ?? "N/A";

                var dto = _mapper.Map<UserMasterDTO>(entity);
                dto.Email = aspUser.Email;
                dto.PhoneNumber = aspUser.PhoneNumber;
                dto.RoleName = role;

                return dto;
            }
            catch (Exception ex)
            {
                throw new Exception(OperationMessages.Error + ex.Message);
            }
        }

        /// <summary>
        /// Retrieves paged user master entities asynchronously.
        /// </summary>
        public async Task<UserMasterEntityPaging> GetPagedAsync(CommonPagingDTO paging, Guid loginUserId)
        {
            try
            {
                var currentUser = await _userManager.FindByIdAsync(loginUserId.ToString());
                if (currentUser == null)
                    throw new Exception("User not found");

                var currentUserRoles = await _userManager.GetRolesAsync(currentUser);

                bool isSuperAdmin = currentUserRoles.Contains("SuperAdmin") || currentUserRoles.Contains("Admin");
                bool isBroker = currentUserRoles.Contains("Broker");

                List<string> visibleRoles = isSuperAdmin
                    ? null
                    : isBroker
                        ? new List<string> { "Landlord", "Tenant" }
                        : new List<string>();

                var query =
                    from u in _context.UserMaster
                    join a in _context.Users on u.AspNetUserIDF.ToString() equals a.Id into userJoin
                    from a in userJoin.DefaultIfEmpty()
                    join ur in _context.UserRoles on a.Id equals ur.UserId into userRoleJoin
                    from ur in userRoleJoin.DefaultIfEmpty()
                    join r in _context.Roles on ur.RoleId equals r.Id into roleJoin
                    from r in roleJoin.DefaultIfEmpty()
                    where !u.IsDelete &&
                          (r == null || r.Name != "SuperAdmin") 
                    select new
                    {
                        u.UserIDP,
                        u.FullName,
                        u.IsActive,
                        u.CreatedDate,
                        a.Email,
                        a.PhoneNumber,
                        RoleName = r.Name,
                        u.CreatedBy,
                        u.AspNetUserIDF
                    };

                // Role-based visibility filters
                if (!isSuperAdmin)
                {
                    if (visibleRoles == null || !visibleRoles.Any())
                        query = query.Where(x => false); 
                    else
                        query = query.Where(x => x.RoleName != null && visibleRoles.Contains(x.RoleName));
                }
                else
                {
                    query = query.Where(x => x.AspNetUserIDF != loginUserId);
                }

                if (isBroker)
                    query = query.Where(x => x.CreatedBy == loginUserId);

                if (!string.IsNullOrWhiteSpace(paging.SearchValue))
                {
                    string search = paging.SearchValue.ToLower();
                    query = query.Where(x =>
                        x.FullName.ToLower().Contains(search) ||
                        (x.Email != null && x.Email.ToLower().Contains(search)) ||
                        (x.PhoneNumber != null && x.PhoneNumber.Contains(search)) ||
                        (x.RoleName != null && x.RoleName.ToLower().Contains(search))
                    );
                }

                var totalCount = await query.CountAsync();

                query = paging.OrderByFieldName?.ToLower() switch
                {
                    "fullname" => (paging.OrderByType == 1 ? query.OrderByDescending(x => x.FullName) : query.OrderBy(x => x.FullName)),
                    "createddate" => (paging.OrderByType == 1 ? query.OrderByDescending(x => x.CreatedDate) : query.OrderBy(x => x.CreatedDate)),
                    "email" => (paging.OrderByType == 1 ? query.OrderByDescending(x => x.Email) : query.OrderBy(x => x.Email)),
                    "phonenumber" => (paging.OrderByType == 1 ? query.OrderByDescending(x => x.PhoneNumber) : query.OrderBy(x => x.PhoneNumber)),
                    "rolename" => (paging.OrderByType == 1 ? query.OrderByDescending(x => x.RoleName) : query.OrderBy(x => x.RoleName)),
                    _ => query.OrderByDescending(x => x.CreatedDate)
                };

                var rawUsers = await query
                    .Skip(paging.DisplayStart)
                    .Take(paging.DisplayLength)
                    .ToListAsync();

                int srNo = paging.DisplayStart + 1;
                var users = rawUsers.Select(x => new UserMasterDTO
                {
                    SrNo = srNo++,
                    UserIDP = x.UserIDP,
                    FullName = x.FullName,
                    IsActive = x.IsActive,
                    CreatedDate = x.CreatedDate.ToString(),
                    Email = x.Email,
                    PhoneNumber = x.PhoneNumber,
                    RoleName = x.RoleName
                }).ToList();

                return new UserMasterEntityPaging
                {
                    Records = users,
                    TotalRecord = totalCount
                };
            }
            catch (Exception ex)
            {
                throw new Exception(OperationMessages.Error + ex.Message);
            }
        }

        /// <summary>
        /// Performs a general action on a user asynchronously (delete or toggle status).
        /// </summary>
        public async Task<bool> GeneralActionAsync(int userId, ActionType actionType)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var entity = await _context.UserMaster.FirstOrDefaultAsync(x => x.UserIDP == userId);
                if (entity == null)
                    throw new Exception(OperationMessages.Error + "User not found.");

                var aspUser = await _userManager.FindByIdAsync(entity.AspNetUserIDF.ToString());
                if (aspUser == null)
                    throw new Exception(OperationMessages.Error + "Linked Identity user not found.");

                switch (actionType)
                {
                    case ActionType.Delete:
                        entity.IsDelete = true;
                        entity.IsActive = false;
                        entity.UpdatedDate = DateTime.UtcNow;
                        _context.UserMaster.Update(entity);

                        aspUser.LockoutEnabled = true;
                        aspUser.LockoutEnd = DateTimeOffset.UtcNow.AddYears(1000);
                        var updateResult = await _userManager.UpdateAsync(aspUser);
                        if (!updateResult.Succeeded)
                            throw new Exception(OperationMessages.Error + string.Join("; ", updateResult.Errors.Select(e => e.Description)));

                        await _context.SaveChangesAsync();
                        break;

                    case ActionType.UpdateStatus:
                        entity.IsActive = !entity.IsActive;
                        entity.UpdatedDate = DateTime.UtcNow;

                        if (!entity.IsActive)
                        {
                            aspUser.LockoutEnabled = true;
                            aspUser.LockoutEnd = DateTimeOffset.UtcNow.AddYears(1000);
                        }
                        else
                        {
                            aspUser.LockoutEnabled = false;
                            aspUser.LockoutEnd = null;
                        }

                        _context.UserMaster.Update(entity);
                        await _userManager.UpdateAsync(aspUser);
                        await _context.SaveChangesAsync();
                        break;

                    default:
                        throw new Exception(OperationMessages.Error + "Invalid action type.");
                }

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception(OperationMessages.Error + ex.Message);
            }
        }
        #endregion

    }
}
