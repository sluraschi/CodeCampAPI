using AutoMapper;
using CodeCampAPI.Models;
using CoreCodeCamp.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeCampAPI.Controllers
{
    [ApiController]
    [Route("api/camps/{moniker}/talks")]
    public class TalksController : ControllerBase
    {
        private readonly ICampRepository _repository;
        private readonly IMapper _mapper;

        public TalksController(ICampRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        // /api/camps/{moniker}/talks
        [HttpGet]
        public async Task<ActionResult<TalkModel[]>> Get(string moniker)
        {
            try
            {
                var talks = await _repository.GetTalksByMonikerAsync(moniker, true);
                return _mapper.Map<TalkModel[]>(talks);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        // /api/camps/{moniker}/talks/N
        [HttpGet("{id:int}")]
        public async Task<ActionResult<TalkModel>> Get(string moniker, int id)
        {
            try
            {
                var talks = await _repository.GetTalkByMonikerAsync(moniker, id, true);
                return _mapper.Map<TalkModel>(talks);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        // POST
        [HttpPost]
        public async Task<ActionResult<TalkModel>> Post(string moniker, [FromBody] TalkModel model)    //[FromBody] es para indicar que viene data y tiene que bindearla a esa entrada
        {
            try
            {
                var existin_campg = await _repository.GetCampAsync(moniker);   // quiero que el Moniker sea unico
                if (existin_campg == null) return BadRequest("Camp does not exist");

                var talk = _mapper.Map<Talk>(model);    // Hago al reves, creo un campo desde un modelo EXPLOTA EL MAPEO
                talk.Camp = existin_campg;

                if (model.Speaker == null) return BadRequest("Speaker ID is rquired");
                var speaker = await _repository.GetSpeakerAsync(model.Speaker.SpeakerId);
                if (speaker == null) return BadRequest("Speaker could not be found");
                talk.Speaker = speaker;

                _repository.Add(talk);  // Y lo meto al repositorio
                if (await _repository.SaveChangesAsync())
                {
                    return Created($"/api/camps/{talk.Camp.Moniker}/{talk.TalkId}", _mapper.Map<TalkModel>(talk));   // Created es un status code 20X, pero no OK, es el que se manda cuando se agrega algo a la API
                                                                                                                     // Deberia usar la clase LinkGenerator en vez de hardcodear la direccion, pero no la podia importar
                }
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
            return BadRequest();
        }

        // PUT api/camps/<moniker>/talks/<id>
        [HttpPut("{id:int}")]
        public async Task<ActionResult<TalkModel>> Put(string moniker, int id, TalkModel model)
        {
            try
            {
                var oldTalk = await _repository.GetTalkByMonikerAsync(moniker, id, true);
                if (oldTalk == null) return NotFound($"Could not find talk from camp {moniker} with id: {id}");

                _mapper.Map(model, oldTalk);

                if (model.Speaker != null)
                {
                    var speaker = await _repository.GetSpeakerAsync(model.Speaker.SpeakerId);
                    if (speaker != null)
                        oldTalk.Speaker = speaker;
                }

                if (await _repository.SaveChangesAsync())
                    return _mapper.Map<TalkModel>(oldTalk);

            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
            return BadRequest();
        }

        // DELETE api/camps/<moniker>/talks/<id>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(string moniker, int id)
        {
            try
            {
                var talk = await _repository.GetTalkByMonikerAsync(moniker, id);
                if (talk == null) return NotFound($"Could not find talk");

                _repository.Delete(talk);
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
