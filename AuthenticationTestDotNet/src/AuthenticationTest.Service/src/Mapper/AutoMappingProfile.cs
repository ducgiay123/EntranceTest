using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthenticationTest.Core.src.Entities;
using AuthenticationTest.Core.src.Utilities;
using AuthenticationTest.Service.src.DTOs;
using AuthenticationTest.src.Core.Entities;
using AutoMapper;
using static AuthenticationTest.Service.src.DTOs.UserDTO;

namespace AuthenticationTest.Service.src.Mapper
{
    public class AutoMappingProfile : Profile
    {
        public AutoMappingProfile()
        {
            CreateMap<Users, UserReadDto>();
            CreateMap<UpdateUserDto, Users>()
                .ForMember(dest => dest.Email, opt => opt.Ignore()) // Don't allow email to be updated this way
                .ForMember(dest => dest.Id, opt => opt.Ignore()) // Don't update ID
                .ForMember(dest => dest.Hash, opt => opt.Ignore()) // Don't update password hash this way
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) // Don't update creation time
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
            CreateMap<SignUpRequestDto, Users>()
                .ForMember(dest => dest.Hash, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Tasks, opt => opt.Ignore());

            CreateMap<Users, SignUpResponseDto>();

            CreateMap<Users, UserDto>()
                            .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));

            CreateMap<Tasks, TaskDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ReverseMap()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse<TasksStatus>(src.Status)));

            CreateMap<CreateTaskDto, Tasks>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ReverseMap();
            // .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse<TasksStatus>(src.Status)));

            // Map UpdateTaskDto to Tasks
            CreateMap<UpdateTaskDto, Tasks>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse<TasksStatus>(src.Status)));
        }
    }
}