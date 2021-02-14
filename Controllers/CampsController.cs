using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CodeCampAPI.Models;
using CoreCodeCamp.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CodeCampAPI.Controllers
{
    [Route("api/[controller]")] // Nuestra ruta es lo que venga antes de Controller en la clase
    [ApiController]
    public class CampsController : ControllerBase  // ControllerBase es una clase especial para APIs
    {
        private readonly ICampRepository _repository;
        private readonly IMapper _mapper;

        public CampsController(ICampRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;   // no quiero devolver mi entidad sino un modelo de la misma, por lo que uso un mapper para crear esos modelos
        }

        // GET api/camps
        [HttpGet]
        public async Task<ActionResult<CampModel[]>> Get(bool includeTalks = false) // Tienen que ser asincronicos porque pueden venir desde muchos lugares al mismo tiempo un GET 
                                                                                    // y tengo que hacer asyncronico el acceso al recurso externo adentro del metodo. 
                                                                                    // Si no es async voy a tener hilos trabados esperando al request y 
                                                                                    // cuando me quede sin hilos todos los GET que me llegen van a terminar en 500 Server Error. 
        {
            try
            {
                // La variable includeTalks se usa para query string, 
                // en la URL puedo poner /api/camps?includeTalks=True (o False)
                // y segun eso devuelve las Talks o no en el json de salida
                var results = await _repository.GetAllCampsAsync(includeTalks); // con el await cuando llega aca devuelve el thread al pool para que no se quede inutil esperando 
                                                                                // a que este request al servicio externo termine. 
                                                                                // Una vez que termina se le asigna de nuevo un thread del pool para continuar con el metodo 

                return _mapper.Map<CampModel[]>(results);   // _mapper es un objeto IMapper, a partir de un Camp crea un modelo de Camp para devolver
                                                            // el mapper va a chequear en los Profiles y usar el correspondiente para mapear esta clase
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        // GET api/camps/search?theDate=<aDate>
        [HttpGet("search")]
        public async Task<ActionResult<CampModel[]>> SearchByDate(DateTime theDate, bool includeTalks = false)
        {
            try
            {
                var results = await _repository.GetAllCampsByEventDate(theDate, includeTalks);
                if (!results.Any()) return NotFound();

                return _mapper.Map<CampModel[]>(results);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }

        }

        // GET api/camps/<moniker>
        [HttpGet("{moniker}")]
        public async Task<ActionResult<CampModel>> Get(string moniker)
        {
            try
            {
                var result = await _repository.GetCampAsync(moniker);
                if (result == null) return NotFound();

                return _mapper.Map<CampModel>(result);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        // POST api/camps
        [HttpPost]
        public async Task<ActionResult<CampModel>> Post([FromBody] CampModel model)    //[FromBody] es para indicar que viene data y tiene que bindearla a esa entrada
        {
            try
            {
                var existing = await _repository.GetCampAsync(model.Moniker);   // quiero que el Moniker sea unico
                if (existing != null) return BadRequest("Moniker in use");

                var camp = _mapper.Map<Camp>(model);    // Hago al reves, creo un campo desde un modelo
                _repository.Add(camp);                  // Y lo meto al repositorio
                if (await _repository.SaveChangesAsync())
                {
                    return Created($"/api/camps/{camp.Moniker}", _mapper.Map<CampModel>(camp));   // Created es un status code 20X, pero no OK, es el que se manda cuando se agrega algo a la API
                                // Deberia usar la clase LinkGenerator en vez de hardcodear la direccion, pero no la podia importar
                }
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
            return BadRequest();
        }

        // PUT api/camps/<moniker>
        [HttpPut("{moniker}")]
        public async Task<ActionResult<CampModel>> Put(string moniker, CampModel model)
        {
            try
            {
                var oldCamp = await _repository.GetCampAsync(moniker);
                if (oldCamp == null) return NotFound($"Could not find camp with moniker: {moniker}");

                _mapper.Map(model, oldCamp);
                if (await _repository.SaveChangesAsync())
                    return _mapper.Map<CampModel>(oldCamp);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
            return BadRequest();
        }

        // DELETE api/camps/<moniker>
        [HttpDelete("{moniker}")]
        public async Task<IActionResult> Delete(string moniker)
        {
            try
            {
                var oldCamp = await _repository.GetCampAsync(moniker);
                if (oldCamp == null) return NotFound($"Could not find camp with moniker: {moniker}");

                _repository.Delete(oldCamp);
                if (await _repository.SaveChangesAsync())
                    return Ok();
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
            return BadRequest();
        }
    }
}
