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
    [Route("api/gamePlayers")]
    [ApiController]
    [Authorize("PlayerOnly")]
    public class GamePlayersController : ControllerBase
    {
        private IGamePlayerRepository _repository;
        private IScoreRepository _scoreRepository;
        private IPlayerRepository _playerRepository;
        public GamePlayersController(IGamePlayerRepository repository, IScoreRepository scoreRepository, IPlayerRepository playerRepository) {
            _repository = repository;
            _scoreRepository = scoreRepository;
            _playerRepository = playerRepository;
        }

        // GET api/<GamePlayersController>/5
        [HttpGet("{id}", Name = "GetGameView")]
        public IActionResult GetGameView(long id)
        {
            try
            {
                string email = User.FindFirst("Player") != null ? User.FindFirst("Player").Value : "Guest";

                var gp = _repository.GetGamePlayerView(id);

                if (gp.Player.Email != email)
                    return Forbid();

                var gameView = new GameViewDTO
                {
                    Id = gp.Id,
                    CreationDate = gp.Game.CreationDate,
                    Ships = gp.Ships.Select(ship => new ShipDTO
                    {
                        Id = ship.Id,
                        Type = ship.Type,
                        Locations = ship.Locations.Select(shipLocation => new ShipLocationDTO
                        {
                            Id = shipLocation.Id,
                            Location = shipLocation.Location
                        }).ToList()
                    }).ToList(),
                    GamePlayers = gp.Game.GamePlayers.Select(gps => new GamePlayerDTO
                    {
                        Id = gps.Id,
                        JoinDate = gps.JoinDate,
                        Player = new PlayerDTO
                        {
                            Id = gps.Player.Id,
                            Email = gps.Player.Email,
                            Avatar = gps.Player.Avatar
                        }
                    }).ToList(),
                    Salvos = gp.Game.GamePlayers.SelectMany(gps => gps.Salvos.Select(salvo => new SalvoDTO
                    {
                        Id = salvo.Id,
                        Turn = salvo.Turn,
                        Player = new PlayerDTO
                        {
                            Id = gps.Player.Id,
                            Email = gps.Player.Email
                        },
                         Locations = salvo.Locations.Select(salvoLocation => new SalvoLocationDTO
                         {
                             Id = salvoLocation.Id,
                             Location = salvoLocation.Location
                         }).ToList()
                    })).ToList(),
                    Hits = gp.GetHits(),
                    HitsOpponent = gp.GetOpponent()?.GetHits(),
                    Sunks = gp.GetSunks(),
                    SunksOpponent = gp.GetOpponent()?.GetSunks(),
                    GameState = Enum.GetName(typeof(GameState), gp.GetGameState())
                };

                return Ok(gameView);
            }
            catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("{id}/ships", Name = "ships")]
        public IActionResult Post(long id, [FromBody] ICollection<ShipDTO> ships)
        {
            try
            {
                string email = User.FindFirst("Player") != null ? User.FindFirst("Player").Value : "Guest";

                var gamePlayer = _repository.FindById(id);

                if (gamePlayer == null)
                    return StatusCode(403, "The game doesn't exist");
                if (gamePlayer.Player.Email != email)
                    return StatusCode(403, "The user is not part of the game");
                if (gamePlayer.Ships.Count == 5)
                    return StatusCode(403, "The ships are already positioned");

                //gamePlayer.Ships = ships.Select(ship => new Ship
                // {
                //   GamePlayerId = gamePlayer.Id,
                //   Type = ship.Type,
                //   Locations = ship.Locations.Select(location => new ShipLocation
                //    {
                //        ShipId = ship.Id,
                //        Location = location.Location
                //    }).ToList()                   
                // }).ToList();               

                foreach (ShipDTO shipDTO in ships)
                {
                    gamePlayer.Ships.Add(new Ship
                    {
                        GamePlayerId = id,
                        Locations = shipDTO.Locations.Select(location => new ShipLocation
                        {
                            Location = location.Location
                        }).ToList(),
                        Type = shipDTO.Type
                    });
                }

                _repository.Save(gamePlayer);

                return StatusCode(201, "Created");
            }
            catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("{id}/salvos", Name = "salvos")]
        public IActionResult Post(long id, [FromBody] SalvoDTO salvo)
        {
            try
            {  
                string email = User.FindFirst("Player") != null ? User.FindFirst("Player").Value : "Guest";

                var gamePlayer = _repository.FindById(id);

                GamePlayer opponentPlayer = gamePlayer.GetOpponent();

                if (gamePlayer == null)
                    return StatusCode(403, "The game doesn't exist");
                if (gamePlayer.Player.Email != email)
                    return StatusCode(403, "The user is not part of the game");
                if (opponentPlayer == null)
                    return StatusCode(403, "You doesnt have an opponent");
                if (gamePlayer.Ships.Count < 5 || opponentPlayer.Ships.Count < 5)
                    return StatusCode(403, "The ships aren't positionated");
                if (gamePlayer.Salvos.Count > opponentPlayer.Salvos.Count)
                    return StatusCode(403, "Is not your turn");
                if (gamePlayer.Salvos.Count == opponentPlayer.Salvos.Count && gamePlayer.JoinDate > opponentPlayer.JoinDate)
                    return StatusCode(403, "Is not your turn");

                if (salvo.Locations.Count != 5)
                    return StatusCode(403, "You just can fire 5 salvos");

                GameState gameState = gamePlayer.GetGameState();

                if (gameState == GameState.LOSS || gameState == GameState.WIN || gameState == GameState.TIE)
                    return StatusCode(403, "Game over");
                
                gamePlayer.Salvos.Add(new Models.Salvo
                {
                    GamePlayerId = id,
                    Turn = gamePlayer.Salvos.Count + 1,
                    Locations = salvo.Locations.Select(location => new SalvoLocation
                    {
                        Location = location.Location
                    }).ToList()
                });

                _repository.Save(gamePlayer);

                gameState = gamePlayer.GetGameState();

                if (gameState == GameState.WIN)
                {
                    Score score = new Score
                    {
                        FinishDate = DateTime.Now,
                        GameId = gamePlayer.GameId,
                        PlayerId = gamePlayer.PlayerId,
                        Point = 1
                    };
                    _scoreRepository.Save(score);

                    Score scoreOpponent = new Score
                    {
                        FinishDate = DateTime.Now,
                        GameId = gamePlayer.GameId,
                        PlayerId = opponentPlayer.PlayerId,
                        Point = 0
                    };
                    _scoreRepository.Save(scoreOpponent);
                }
                else if (gameState == GameState.LOSS)
                {
                    Score score = new Score
                    {
                        FinishDate = DateTime.Now,
                        GameId = gamePlayer.GameId,
                        PlayerId = gamePlayer.PlayerId,
                        Point = 0
                    };
                    _scoreRepository.Save(score);

                    Score scoreOpponent = new Score
                    {
                        FinishDate = DateTime.Now,
                        GameId = gamePlayer.GameId,
                        PlayerId = opponentPlayer.PlayerId,
                        Point = 1
                    };
                    _scoreRepository.Save(scoreOpponent);
                }
                else if (gameState == GameState.TIE)
                {
                    Score score = new Score
                    {
                        FinishDate = DateTime.Now,
                        GameId = gamePlayer.GameId,
                        PlayerId = gamePlayer.PlayerId,
                        Point = 0.5
                    };
                    _scoreRepository.Save(score);

                    Score scoreOpponent = new Score
                    {
                        FinishDate = DateTime.Now,
                        GameId = gamePlayer.GameId,
                        PlayerId = opponentPlayer.PlayerId,
                        Point = 0.5
                    };
                    _scoreRepository.Save(scoreOpponent);
                }
                return StatusCode(201);
            }
            catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }

}
