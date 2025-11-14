using AutoMapper;
using RealEstate.Application.DTOs;
using RealEstate.Core.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealEstate.Application.Mapping
{
    public class PropertyTypeMasterMappingProfile : Profile
    {
        public PropertyTypeMasterMappingProfile()
        {
            /// <summary>
            /// Maps PropertyTypeSaveDTO to PropertyTypeEntity bidirectionally,
            /// mapping CreatedBy from UserIDF and ignoring UpdatedDate.
            /// </summary>
            CreateMap<PropertyTypeMasterSaveDTO, PropertyTypeMasterEntity>()
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.UserIDF))
                .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                .ReverseMap();

            /// <summary>
            /// Maps PropertyTypeEntity to PropertyTypeDTO bidirectionally,
            /// formatting CreatedDate to dd-MM-yyyy.
            /// </summary>
            CreateMap<PropertyTypeMasterEntity, PropertyTypeMasterDTO>()
                .ForMember(dest => dest.CreatedDate,
                           opt => opt.MapFrom(src =>
                               src.CreatedDate.HasValue
                               ? src.CreatedDate.Value.ToString("dd-MM-yyyy")
                               : string.Empty))
                .ReverseMap();
        }
    }
}
