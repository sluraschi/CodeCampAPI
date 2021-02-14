using AutoMapper;
using CodeCampAPI.Models;
using CoreCodeCamp.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeCampAPI.Data
{
    public class CampProfile : Profile  // Profile esta en el using AutoMapper
    {
        public CampProfile()
        {
            this.CreateMap<Camp, CampModel>()   // Como estoy usando los mismos nombres, esto ya el alcanza al mapper para saber como mapear de un objeto al otro
                .ForMember(c => c.Venue, o => o.MapFrom(m => m.Location.VenueName)) // En Venue mappea desde el Camp object de Location VenueName 
                                                                                    // (o algo asi, no se, cuestion que mete en Venue el nombre de la Location dentro de ese objeto camp)
                .ReverseMap();                  // Crea el mapa inverso, para que funcione el POST
        }
    }
}
