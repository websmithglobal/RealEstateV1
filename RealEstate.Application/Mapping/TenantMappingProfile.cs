using AutoMapper;
using RealEstate.Application.DTOs;
using RealEstate.Core.Data.Entities;

namespace RealEstate.Application.Mapping
{

    /// <summary>
    /// Represents an AutoMapper profile for mapping between tenant-related DTOs and entities.
    /// Created By - Nirmal
    /// Created Date - 13.11.2025
    /// </summary>
    public class TenantMappingProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the TenantMappingProfile and configures the mapping rules.
        /// Created By - Nirmal
        /// Created Date - 13.11.2025
        /// </summary>
        public TenantMappingProfile()
        {
            /// <summary>
            /// Maps TenantMasterSaveDTO to TenantMasterEntity bidirectionally, setting CreatedBy and ignoring UpdatedDate.
            /// </summary>
            CreateMap<TenantMasterSaveDTO, TenantMasterEntity>()
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.UserIDF))
                .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                .ReverseMap();

            /// <summary>
            /// Maps TenantMasterEntity to TenantMasterDTO bidirectionally, formatting CreatedDate as string.
            /// </summary>
            CreateMap<TenantMasterEntity, TenantMasterDTO>()
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate.HasValue ? src.CreatedDate.Value.ToString("dd-MM-yyyy") : string.Empty))
                .ReverseMap();
        }
    }
}

