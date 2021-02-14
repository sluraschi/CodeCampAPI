using AutoMapper;
using CodeCampAPI.Models;
using CoreCodeCamp.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeCampAPI.Data.Profiles
{
    public class SpeakerProfile : Profile
    {
        public SpeakerProfile()
        {
            this.CreateMap<Speaker, SpeakerModel>().ReverseMap();
        }
    }
}
