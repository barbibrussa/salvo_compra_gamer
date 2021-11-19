using Microsoft.AspNetCore.Authorization;
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
    [Authorize]
    public class GamesController : ControllerBase
    {
        private IGameRepository _repository;
        private IPlayerRepository _playerRepository;
        private IGamePlayerRepository _gamePlayerRepository;
        public GamesController(IGameRepository repository, IPlayerRepository playerRepository, IGamePlayerRepository gamePlayerRepository)
        {
            _repository = repository;
            _playerRepository = playerRepository;
            _gamePlayerRepository = gamePlayerRepository;
        }

        // GET: api/<GamesController>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Get()
        {
            try 
            {
                GameListDTO gameList = new GameListDTO
                {
                    Email = User.FindFirst("Player") != null ? User.FindFirst("Player").Value : "Guest",
                    Games = _repository.GetAllGamesWithPlayers()
                    .Select(g => new GameDTO
                    {
                        Id = g.Id,
                        CreationDate = g.CreationDate,
                        GamePlayers = g.GamePlayers.Select(gp => new GamePlayerDTO
                        {
                            Id = gp.Id,
                            JoinDate = gp.JoinDate,
                            Player = new PlayerDTO
                            {
                                Id = gp.Player.Id,
                                Email = gp.Player.Email
                            },
                            Point = gp.GetScore() != null ? (double?)gp.GetScore().Point : null
                        }).ToList()
                    }).ToList()
                };

                return Ok(gameList);
            } 
            catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Post()
        {
            try
            {
                string email = User.FindFirst("Player") != null ? User.FindFirst("Player").Value : "Guest";

                Player player = _playerRepository.FindByEmail(email);
                DateTime currentDate = DateTime.Now;
                GamePlayer gamePlayer = new GamePlayer
                {
                    Game = new Game
                    {
                        CreationDate = currentDate,
                    },
                    PlayerId = player.Id,
                    JoinDate = currentDate,
                };

                _gamePlayerRepository.Save(gamePlayer);

                return StatusCode(201, gamePlayer.Id);
            }
            catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("{id}/players", Name = "Join")]
        public IActionResult Join(long id)
        {
            try
            {
                string email = User.FindFirst("Player") != null ? User.FindFirst("Player").Value : "Guest";

                Player player = _playerRepository.FindByEmail(email);
                Game game = _repository.FindById(id);

                if (game == null)
                {
                    return StatusCode(403, "The game doesn't exist");
                }

                if (game.GamePlayers.Where(gp => gp.Player.Id == player.Id).FirstOrDefault() != null)
                    return StatusCode(403, "The player is on the game already");

                if (game.GamePlayers.Count > 1)
                    return StatusCode(403, "The game is full");

                GamePlayer gamePlayer = new GamePlayer
                {
                    GameId = game.Id,
                    PlayerId = player.Id,
                    JoinDate = DateTime.Now
                };

                _gamePlayerRepository.Save(gamePlayer);

                return StatusCode(201, gamePlayer.Id);
            }
            catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
