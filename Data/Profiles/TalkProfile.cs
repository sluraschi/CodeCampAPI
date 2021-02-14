using AutoMapper;
using CodeCampAPI.Models;
using CoreCodeCamp.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeCampAPI.Data
{
    public class TalkProfile : Profile
    {
        public TalkProfile()
        {
            this.CreateMap<Talk, TalkModel>()
                .ReverseMap()
                .ForMember(t => t.Camp, opt => opt.Ignore())   // Al ponerlo despues de reverse map, el ignore solo lo va a hacer cuando vamos de TalkModel a Talk
                .ForMember(t => t.Speaker, opt => opt.Ignore());   // Al ponerlo despues de reverse map, el ignore solo lo va a hacer cuando vamos de TalkModel a Talk
        }
    }
}
