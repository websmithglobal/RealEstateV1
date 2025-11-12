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
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

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

        /// <summary>
        /// Retrieves paged user master entities asynchronously based on paging parameters and the logged-in user ID, applying role-based visibility filters.
        /// Created By - Nirmal
        /// Created Date - 12.11.2025
        /// </summary>
        /// <param name="paging">The paging, sorting, and search parameters.</param>
        /// <param name="loginUserId">The ID of the logged-in user for role-based filtering.</param>
        /// <returns>A task representing the asynchronous operation, returning the paged user master entities.</returns>
        public async Task<UserMasterEntityPaging> GetPagedAsync(CommonPagingDTO paging, Guid loginUserId)
        {
            // Get current user by ID
            var currentUser = await _userManager.FindByIdAsync(loginUserId.ToString());
            if (currentUser == null) throw new Exception("User not found");
            // Get current user's roles
            var currentUserRoles = await _userManager.GetRolesAsync(currentUser);
            // Determine visible roles
            List<string> visibleRoles = new List<string>();
            bool isSuperAdmin = currentUserRoles.Contains("SuperAdmin");
            bool isBroker = currentUserRoles.Contains("Broker");
            if (isSuperAdmin)
            {
                // SuperAdmin sees all users except themselves
                visibleRoles = null; // null means no filter
            }
            else if (isBroker)
            {
                // Broker sees only Landlord and Tenant
                visibleRoles = new List<string> { "Landlord", "Tenant" };
            }
            // Base query with joins
            var query = from u in _context.UserMaster
                        join a in _context.Users
                            on (u.AspNetUserIDF != null ? u.AspNetUserIDF.ToString() : null) equals a.Id into userJoin
                        from a in userJoin.DefaultIfEmpty()
                        join ur in _context.UserRoles
                            on a.Id equals ur.UserId into userRoleJoin
                        from ur in userRoleJoin.DefaultIfEmpty()
                        join r in _context.Roles
                            on ur.RoleId equals r.Id into roleJoin
                        from r in roleJoin.DefaultIfEmpty()
                        select new { u, a, r };
            // Filter by visible roles
            if (!isSuperAdmin && visibleRoles.Any())
            {
                query = query.Where(x => x.r != null && visibleRoles.Contains(x.r.Name));
            }
            // Exclude self for SuperAdmin
            if (isSuperAdmin)
            {
                query = query.Where(x => x.u.AspNetUserIDF != loginUserId);
            }
            // Apply search
            if (!string.IsNullOrEmpty(paging.SearchValue))
            {
                string search = paging.SearchValue.ToLower();
                query = query.Where(x =>
                    x.u.FullName.ToLower().Contains(search) ||
                    (x.a.Email != null && x.a.Email.ToLower().Contains(search)) ||
                    (x.a.PhoneNumber != null && x.a.PhoneNumber.Contains(search)) ||
                    (x.r.Name != null && x.r.Name.ToLower().Contains(search))
                );
            }
            var totalCount = await query.CountAsync();
            // Apply ordering
            if (!string.IsNullOrEmpty(paging.OrderByFieldName))
            {
                switch (paging.OrderByFieldName.ToLower())
                {
                    case "fullname":
                        query = paging.OrderByType == 1 ? query.OrderByDescending(x => x.u.FullName) : query.OrderBy(x => x.u.FullName);
                        break;
                    case "createddate":
                        query = paging.OrderByType == 1 ? query.OrderByDescending(x => x.u.CreatedDate) : query.OrderBy(x => x.u.CreatedDate);
                        break;
                    case "email":
                        query = paging.OrderByType == 1 ? query.OrderByDescending(x => x.a.Email) : query.OrderBy(x => x.a.Email);
                        break;
                    case "phonenumber":
                        query = paging.OrderByType == 1 ? query.OrderByDescending(x => x.a.PhoneNumber) : query.OrderBy(x => x.a.PhoneNumber);
                        break;
                    case "rolename":
                        query = paging.OrderByType == 1 ? query.OrderByDescending(x => x.r.Name) : query.OrderBy(x => x.r.Name);
                        break;
                    default:
                        query = query.OrderByDescending(x => x.u.CreatedDate);
                        break;
                }
            }
            else
            {
                query = query.OrderByDescending(x => x.u.CreatedDate);
            }
            // Fetch paged data
            var rawUsers = await query
                .Skip(paging.DisplayStart)
                .Take(paging.DisplayLength)
                .Select(x => new
                {
                    x.u.UserIDP,
                    x.u.FullName,
                    x.u.IsActive,
                    x.u.CreatedDate,
                    x.a.Email,
                    x.a.PhoneNumber,
                    RoleName = x.r.Name
                })
                .ToListAsync();
            int srNo = paging.DisplayStart + 1;
            var users = rawUsers.Select(x => new UserMasterDTO
            {
                UserIDP = x.UserIDP,
                FullName = x.FullName,
                IsActive = x.IsActive,
                CreatedDate = x.CreatedDate.ToString(),
                Email = x.Email,
                PhoneNumber = x.PhoneNumber,
                RoleName = x.RoleName,
                SrNo = srNo++
            }).ToList();
            return new UserMasterEntityPaging
            {
                Records = users,
                TotalRecord = totalCount
            };
        }

        /// <summary>
        /// Saves or updates a user master entity asynchronously, handling both insert and update modes with Identity user management.
        /// Created By - Nirmal
        /// Created Date - 12.11.2025
        /// </summary>
        /// <param name="dto">The data transfer object containing user details to save or update.</param>
        /// <returns>A task representing the asynchronous operation, returning true if successful.</returns>
        public async Task<SQLReturnMessageNValue> SaveAsync(UserMasterSaveDTO dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Check for existing email or phone to give a warning (99)
                var existingUser = await _context.Users
                    .Where(u => u.Email == dto.Email || u.PhoneNumber == dto.PhoneNumber)
                    .FirstOrDefaultAsync();

                if (existingUser != null && (dto.UserIDP == null || existingUser.Id != dto.UserIDP.ToString()))
                {
                    return new SQLReturnMessageNValue
                    {
                        Outval = 99,
                        Outmsg = "Email or phone number already exists."
                    };
                }

                // INSERT MODE
                if (dto.UserIDP == null || dto.UserIDP == 0)
                {
                    var applicationUser = _mapper.Map<ApplicationUser>(dto);
                    var createResult = await _userManager.CreateAsync(applicationUser, dto.Password ?? "Default@123");

                    if (!createResult.Succeeded)
                    {
                        return new SQLReturnMessageNValue
                        {
                            Outval = 0,
                            Outmsg = string.Join("; ", createResult.Errors.Select(e => e.Description))
                        };
                    }

                    // Ensure Role exists
                    if (!await _roleManager.RoleExistsAsync(dto.Role))
                    {
                        var roleResult = await _roleManager.CreateAsync(new IdentityRole(dto.Role));
                        if (!roleResult.Succeeded)
                        {
                            return new SQLReturnMessageNValue
                            {
                                Outval = 0,
                                Outmsg = "Role creation failed: " + string.Join("; ", roleResult.Errors.Select(e => e.Description))
                            };
                        }
                    }

                    // Assign Role
                    var addRoleResult = await _userManager.AddToRoleAsync(applicationUser, dto.Role);
                    if (!addRoleResult.Succeeded)
                    {
                        return new SQLReturnMessageNValue
                        {
                            Outval = 0,
                            Outmsg = "Role assignment failed: " + string.Join("; ", addRoleResult.Errors.Select(e => e.Description))
                        };
                    }

                    // Map DTO + Identity → Entity
                    var entity = _mapper.Map<UserMasterEntity>(dto);
                    _mapper.Map(applicationUser, entity);
                    entity.CreatedBy = dto.UserIDF;

                    _context.UserMaster.Add(entity);
                    await _context.SaveChangesAsync();
                }
                // UPDATE MODE
                else
                {
                    var entity = await _context.UserMaster.FirstOrDefaultAsync(u => u.UserIDP == dto.UserIDP);
                    if (entity == null)
                    {
                        return new SQLReturnMessageNValue { Outval = 0, Outmsg = "User not found in UserMaster." };
                    }

                    var aspUser = await _userManager.FindByIdAsync(entity.AspNetUserIDF.ToString());
                    if (aspUser == null)
                    {
                        return new SQLReturnMessageNValue { Outval = 0, Outmsg = "Associated AspNetUser not found." };
                    }

                    _mapper.Map(dto, aspUser);
                    var updateResult = await _userManager.UpdateAsync(aspUser);
                    if (!updateResult.Succeeded)
                    {
                        return new SQLReturnMessageNValue
                        {
                            Outval = 0,
                            Outmsg = string.Join("; ", updateResult.Errors.Select(e => e.Description))
                        };
                    }

                    if (!string.IsNullOrEmpty(dto.Password))
                    {
                        var token = await _userManager.GeneratePasswordResetTokenAsync(aspUser);
                        var resetResult = await _userManager.ResetPasswordAsync(aspUser, token, dto.Password);
                        if (!resetResult.Succeeded)
                        {
                            return new SQLReturnMessageNValue
                            {
                                Outval = 0,
                                Outmsg = string.Join("; ", resetResult.Errors.Select(e => e.Description))
                            };
                        }
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
                }

                await transaction.CommitAsync();

                return new SQLReturnMessageNValue
                {
                    Outval = 1,
                    Outmsg = "Saved successfully."
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new SQLReturnMessageNValue
                {
                    Outval = 0,
                    Outmsg = $"User save failed: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Retrieves a user master entity by its ID asynchronously, including associated Identity user and role details.
        /// Created By - Nirmal
        /// Created Date - 12.11.2025
        /// </summary>
        /// <param name="userID">The ID of the user to retrieve.</param>
        /// <returns>A task representing the asynchronous operation, returning the user master details.</returns>
        public async Task<UserMasterDTO> GetByIdAsync(int userID)
        {
            // 1️⃣ Get UserMaster record
            var entity = await _context.UserMaster.FirstOrDefaultAsync(x => x.UserIDP == userID);
            if (entity == null)
                throw new Exception("User not found.");
            // 2️⃣ Get linked ApplicationUser
            var aspUser = await _userManager.FindByIdAsync(entity.AspNetUserIDF.ToString());
            if (aspUser == null)
                throw new Exception("Associated AspNetUser not found.");
            // 3️⃣ Get Role info
            var roles = await _userManager.GetRolesAsync(aspUser);
            string role = roles.FirstOrDefault() ?? "N/A";
            // 4️⃣ Map using AutoMapper
            var dto = _mapper.Map<UserMasterDTO>(entity);
            // 5️⃣ Fill Identity fields
            dto.Email = aspUser.Email;
            dto.PhoneNumber = aspUser.PhoneNumber;
            dto.RoleName = role;
            return dto;
        }

        /// <summary>
        /// Performs a general action on a user asynchronously, such as deletion or status toggling.
        /// Created By - Nirmal
        /// Created Date - 12.11.2025
        /// </summary>
        /// <param name="userId">The ID of the user to perform the action on.</param>
        /// <param name="actionType">The type of action to perform (e.g., Delete, UpdateStatus).</param>
        /// <returns>A task representing the asynchronous operation, returning true if successful.</returns>
        public async Task<bool> GeneralActionAsync(int userId, ActionType actionType)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1️⃣ Find user in UserMaster table
                var entity = await _context.UserMaster.FirstOrDefaultAsync(x => x.UserIDP == userId);
                if (entity == null)
                    throw new Exception("User not found.");
                // 2️⃣ Find linked AspNetUser
                var aspUser = await _userManager.FindByIdAsync(entity.AspNetUserIDF.ToString());
                if (aspUser == null)
                    throw new Exception("Linked Identity user not found.");
                switch (actionType)
                {
                    // 🧹 Delete user (both from UserMaster & Identity)
                    case ActionType.Delete:
                        // Remove from AspNetUsers
                        var deleteResult = await _userManager.DeleteAsync(aspUser);
                        if (!deleteResult.Succeeded)
                            throw new Exception(string.Join("; ", deleteResult.Errors.Select(e => e.Description)));
                        // Remove from UserMaster
                        _context.UserMaster.Remove(entity);
                        await _context.SaveChangesAsync();
                        break;
                    // 🔄 Toggle Active Status
                    case ActionType.UpdateStatus:
                        entity.IsActive = !entity.IsActive;
                        entity.UpdatedDate = DateTime.UtcNow;
                        if (!entity.IsActive)
                        {
                            // Lock the user for 1000 years
                            aspUser.LockoutEnabled = true;
                            aspUser.LockoutEnd = DateTimeOffset.UtcNow.AddYears(1000);
                        }
                        else
                        {
                            // Unlock user if reactivated
                            aspUser.LockoutEnabled = false;
                            aspUser.LockoutEnd = null;
                        }
                        _context.UserMaster.Update(entity);
                        await _context.SaveChangesAsync();
                        break;
                    default:
                        throw new Exception("Invalid action type.");
                }
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"Operation failed: {ex.Message}");
            }
        }
    }
}
