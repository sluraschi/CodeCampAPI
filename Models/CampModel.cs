using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CodeCampAPI.Models
{
    public class CampModel
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        [Required]
        public string Moniker { get; set; }
        public int Length { get; set; } = 1;
        public DateTime EventDate { get; set; } = DateTime.MinValue;

        // En lugar de crear un modelo de la calse Location puedo extender el modelo de Camp.
        // En el profile le digo como mapear el Venue a partir de la entidad Location dentro de la entidad Camp
        public string Venue { get; set; }
        // Otra opcion: al agregarle Location adelante el mapper ya se da cuenta que pertence al objeto Location que tiene la instancia de Camp y sabe de donde sacar los datos para mapearlo solo
        public string LocationCountry { get; set; }
        
        public ICollection<TalkModel> Talks { get; set; }
    }
}
