using BestMusPortal.Models;
using BestMusPortal.Services.DTO;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BestMusPortal.Services.Mapping
{
    public class MappingProfile : Profile
    {

        public MappingProfile()
        {
            CreateMap<User, UserDTO>().ReverseMap();
            CreateMap<SongDTO, Song>()
            .ForMember(dest => dest.Genre, opt => opt.MapFrom(src => src.Genre))
            .ForMember(dest => dest.GenreId, opt => opt.MapFrom(src => src.GenreId));

            CreateMap<Song, SongDTO>()
                .ForMember(dest => dest.Genre, opt => opt.MapFrom(src => src.Genre))
                .ForMember(dest => dest.GenreId, opt => opt.MapFrom(src => src.GenreId));


            CreateMap<Genre, GenreDTO>().ReverseMap();
        }
    }
}