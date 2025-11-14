using AutoMapper;
using RealEstate.Application.DTOs;
using RealEstate.Core.Data.Entities;

namespace RealEstate.Application.Mapping
{

    /// <summary>
    /// AutoMapper profile for Building Type mappings.
    /// Maps between Entity ↔ DTO ↔ SaveDTO.
    /// </summary>
    public class BuildingTypeMasterMappingProfile : Profile
    {
        public BuildingTypeMasterMappingProfile()
        {
            /// <summary>
            /// Maps BuildingTypeSaveDTO to BuildingTypeEntity bidirectionally, 
            /// Created By - Prashant
            /// Created Date - 14.11.2025
            /// </summary>
            CreateMap<BuildingTypeMasterSaveDTO, BuildingTypeMasterEntity>()
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.UserIDF))
                .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                .ReverseMap();

            /// <summary>
            /// Maps BuildingTypeEntity to BuildingTypeDTO bidirectionally, 
            /// Created By - Prashant
            /// Created Date - 14.11.2025
            /// </summary>
            CreateMap<BuildingTypeMasterEntity, BuildingTypeMasterDTO>()
                .ForMember(dest => dest.CreatedDate,
                           opt => opt.MapFrom(src =>
                               src.CreatedDate.HasValue
                               ? src.CreatedDate.Value.ToString("dd-MM-yyyy")
                               : string.Empty))
                .ForMember(dest => dest.PropertyTypeName,
                           opt => opt.MapFrom(src => src.PropertyTypeMaster.PropertyTypeName))
                .ReverseMap();
        }
    }
}
