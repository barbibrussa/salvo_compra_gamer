using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Salvo.Models;
using Salvo.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
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
            bool InvalidField = false;
            string ErrorMessage = "", ErrorPasswordMessage = "";

            try
            {
                if (String.IsNullOrEmpty(player.Email) && String.IsNullOrEmpty(player.Password))
                {
                    InvalidField = true;
                    //return StatusCode(403, "Can't be empy spaces");
                    ErrorMessage = (string.IsNullOrEmpty(ErrorMessage)) ? "Can't be empy spaces" : ErrorMessage + " - Can't be empy spaces";
                }

                if (String.IsNullOrEmpty(player.Email))
                {
                    InvalidField = true;
                    //return StatusCode(403, "Email can't be blank");
                    ErrorMessage = (string.IsNullOrEmpty(ErrorMessage)) ? "The Email cannot be Empty" : ErrorMessage + " - The Email cannot be Empty";
                }


                if (String.IsNullOrEmpty(player.Name))
                {
                    InvalidField = true;
                    //return StatusCode(403, "Name can't be blank");
                    ErrorMessage = (string.IsNullOrEmpty(ErrorMessage)) ? "The Name cannot be Empty" : ErrorMessage + " - The Name cannot be Empty";
                }


                if (String.IsNullOrEmpty(player.Password))
                {
                    InvalidField = true;
                    //return StatusCode(403, "Password can't be blank");
                    ErrorMessage = (string.IsNullOrEmpty(ErrorMessage)) ? "The Password cannot be Empty" : ErrorMessage + " - The Password cannot be Empty";
                }

                if (!isValidEmail(player.Email))
                {
                    InvalidField = true;
                    ErrorMessage = (string.IsNullOrEmpty(ErrorMessage)) ? "The Email is not valid" : ErrorMessage + " - The Email is not valid";
                }

                if (!_repository.ValidatePassword(player.Password, out ErrorMessage))
                {
                    InvalidField = true;
                    ErrorMessage = (string.IsNullOrEmpty(ErrorMessage)) ? ErrorPasswordMessage : ErrorMessage + " - " + ErrorPasswordMessage;
                    //return StatusCode(403, ErrorMessage);
                }
                    
                if (!InvalidField)
                {
                    Player newPlayer = _repository.FindByEmail(player.Email);
                    if (newPlayer == null)
                    {
                        byte[] data = Encoding.ASCII.GetBytes(player.Password);
                        data = new SHA256Managed().ComputeHash(data);
                        string passwordHash = Encoding.ASCII.GetString(data);

                        newPlayer = new Player
                        {
                            Name = player.Name,
                            Email = player.Email,
                            Password = passwordHash,
                            Avatar = "1.jpg"
                        };

                        _repository.Save(newPlayer);

                        return StatusCode(201, "Creado");
                    }
                    else
                    {
                        return StatusCode(403, "Email en uso");
                    }
                }
                else
                {
                    return StatusCode(403, ErrorMessage);
                }

            }
            catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        public bool isValidName(string Name)
        {
            var hasBetween3And30Chars = new Regex(@".{3,30}");

            return hasBetween3And30Chars.IsMatch(Name);
        }
        public bool isValidEmail(string Email)
        {
            var hasBetween10And30Chars = new Regex(@".{10,30}");

            bool hasValidFormat = true;

            if (Email == null || Email == "")
            {
                hasValidFormat = false;
                return hasValidFormat;
            }

            try
            {
                MailAddress address = new MailAddress(Email);
                hasValidFormat = (address.Address == Email);
                // or
                // isValid = string.IsNullOrEmpty(address.DisplayName);
            }
            catch (FormatException)
            {
                hasValidFormat = false;
            }

            return hasValidFormat && hasBetween10And30Chars.IsMatch(Email);
        }
    }
}
