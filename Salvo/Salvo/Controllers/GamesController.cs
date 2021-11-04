using Microsoft.AspNetCore.Mvc;
using Salvo.Models;
using Salvo.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Salvo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GamesController : ControllerBase
    {
        private IGameRepository _repository;
        public GamesController(IGameRepository repository)
        {
            _repository = repository;
        }

        // GET: api/<GamesController>
        [HttpGet]
        public IActionResult Get()
        {
            try 
            {
                var games = _repository.GetAllGamesWithPlayers()
                    .Select(g=> new GameDTO { 
                        Id = g.Id,
                        CreationDate = g.CreationDate,
                        GamePlayers = g.GamePlayers.Select(gb => new GamePlayerDTO
                        {
                            Id = gb.Id,
                            JoinDate = gb.JoinDate,
                            Player = new PlayerDTO
                            {
                                Id = gb.Player.Id,
                                Email = gb.Player.Email
                            }
                        }).ToList()
                    });
                return Ok(games);
            } 
            catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // GET api/<GamesController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<GamesController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<GamesController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<GamesController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
