using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Salvo.Models;
using Salvo.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Salvo.Controllers
{
    [Route("api/players")]
    [ApiController]
    public class PlayersController : ControllerBase
    {
        private IPlayerRepository _repository;

        public PlayersController(IPlayerRepository repository)
        {
            _repository = repository;
        }

        [HttpPost]
        public IActionResult Post([FromBody] PlayerDTO player)
        {
            string ErrorMessage;

            try
            {
                if (String.IsNullOrEmpty(player.Email) && String.IsNullOrEmpty(player.Password))
                    return StatusCode(403, "Can't be empy spaces");

                if (String.IsNullOrEmpty(player.Email))
                    return StatusCode(403, "Email can't be blank");

                if (String.IsNullOrEmpty(player.Name))
                    return StatusCode(403, "Name can't be blank");

                if (String.IsNullOrEmpty(player.Password))
                    return StatusCode(403, "Password can't be blank");

                if (!_repository.ValidatePassword(player.Password, out ErrorMessage))
                    return StatusCode(403, ErrorMessage);

                Player dbPlayer = _repository.FindByEmail(player.Email);
                if (dbPlayer != null)
                    return StatusCode(403, "Email already exists");

                Player newPlayer = new Player
                {
                    Email = player.Email,
                    Password = player.Password,
                    Name = player.Name
                };

                _repository.Save(newPlayer);

                return StatusCode(201, newPlayer);
            }
            catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
