using AutoMapper;
using RealEstate.Application.DTOs;
using RealEstate.Core.Identity;
using RealEstate.Infrastructure.Data.Entities;
namespace RealEstate.Application.Mapping
{
    /// <summary>
    /// Represents an AutoMapper profile for mapping between user-related DTOs, entities, and IdentityUser objects.
    /// Created By - Nirmal
    /// Created Date - 12.11.2025
    /// </summary>
    public class UserMappingProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the UserMappingProfile and configures the mapping rules.
        /// Created By - Nirmal
        /// Created Date - 12.11.2025
        /// </summary>
        public UserMappingProfile()
        {
            // DTO → IdentityUser
            /// <summary>
            /// Maps UserMasterSaveDTO to ApplicationUser for Identity user creation.
            /// </summary>
            CreateMap<UserMasterSaveDTO, ApplicationUser>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email));
            /// <summary>
            /// Maps UserMasterEntity to UserMasterDTO bidirectionally.
            /// </summary>
            CreateMap<UserMasterEntity, UserMasterDTO>().ReverseMap();
            // DTO → Entity (insert + update)
            /// <summary>
            /// Maps UserMasterSaveDTO to UserMasterEntity for insert/update, with defaults for new records.
            /// </summary>
            CreateMap<UserMasterSaveDTO, UserMasterEntity>()
                .ForMember(dest => dest.UserIDP, opt => opt.MapFrom(src => src.UserIDP ?? 0))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.UserIDF))
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                .ReverseMap();
            // IdentityUser → Entity link (for insert)
            /// <summary>
            /// Maps ApplicationUser to UserMasterEntity to link Identity ID.
            /// </summary>
            CreateMap<ApplicationUser, UserMasterEntity>()
                .ForMember(dest => dest.AspNetUserIDF, opt => opt.MapFrom(src => Guid.Parse(src.Id)));
        }
    }
}