using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salvo.Models;
using Salvo.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Salvo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize("PlayerOnly")]
    public class SettingsController : Controller
    {
        private IPlayerRepository _repository;

        public SettingsController(IPlayerRepository repository, IGameRepository gameRepository)
        {
            _repository = repository;

        }

        [HttpGet("permisse")]
        [AllowAnonymous]
        public IActionResult GetPermisses()
        {
            if (User.FindFirst("Player") != null)
            {
                return Ok("Usuario permitido");
            }
            else
            {
                //Unauthorized()
                return Forbid("Usuario denegado");
            }


        }

        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                string email = User.FindFirst("Player") != null ? User.FindFirst("Player").Value : "Guest";

                if (email == "Guest")
                {
                    return StatusCode(403, "No tiene permisos");
                }

                Player dbPlayer = _repository.FindByEmail(email);

                if (dbPlayer == null)
                {
                    return StatusCode(403, "Usuario no registrado");
                }

                PlayerDTO player = new PlayerDTO
                {
                    Name = dbPlayer.Name,
                    Email = email,
                    Avatar = dbPlayer.Avatar
                };

                return Ok(player);
            }
            catch (Exception ex)
            {
                return StatusCode(403, ex.Message);
            }



        }

        [HttpPost("name")]
        public IActionResult ChangeName([FromBody] PlayerDTO player)
        {
            try
            {
                string MensajeError = "";
                bool CamposInvalidos = false;
                string email = User.FindFirst("Player") != null ? User.FindFirst("Player").Value : "Guest";

                if (email == "Guest")
                {
                    CamposInvalidos = true;
                    MensajeError = "Usuario no registrado";
                }

                if (string.IsNullOrEmpty(player.Name))
                {
                    CamposInvalidos = true;
                    MensajeError = (string.IsNullOrEmpty(MensajeError)) ? "The Name cannot be Empty" : MensajeError + " - The Name cannot be Empty";
                }
                if (!isValidName(player.Name))
                {
                    CamposInvalidos = true;
                    MensajeError = (string.IsNullOrEmpty(MensajeError)) ? "The Name is not valid" : MensajeError + " - The Name is not valid";
                }

                Player dbPlayer = _repository.FindByEmail(email);

                if (dbPlayer == null)
                {
                    CamposInvalidos = true;
                    MensajeError = "Email no registrado";
                }


                byte[] data = Encoding.ASCII.GetBytes(player.Password);
                data = new SHA256Managed().ComputeHash(data);
                string passwordHash = Encoding.ASCII.GetString(data);

                if (passwordHash != dbPlayer.Password)
                {
                    CamposInvalidos = true;
                    MensajeError = "Contraseña incorrecta";
                }

                if (!CamposInvalidos)
                {

                    Player newPlayer = new Player
                    {
                        Id = dbPlayer.Id,
                        Name = player.Name,
                        Email = email,
                        Password = dbPlayer.Password,
                        Avatar = dbPlayer.Avatar
                    };

                    _repository.Save(newPlayer);

                    return StatusCode(201, newPlayer);
                }
                else
                {
                    return StatusCode(403, MensajeError);
                }

            }
            catch (Exception ex)
            {
                return StatusCode(403, ex.Message);
            }
        }

        [HttpPost("mail")]
        public async Task<IActionResult> ChangeMail([FromBody] PlayerDTO player)
        {
            try
            {
                string MensajeError = "";
                bool CamposInvalidos = false;
                string email = User.FindFirst("Player") != null ? User.FindFirst("Player").Value : "Guest";

                if (string.IsNullOrEmpty(player.Email))
                {
                    CamposInvalidos = true;
                    MensajeError = (string.IsNullOrEmpty(MensajeError)) ? "The Email cannot be Empty" : MensajeError + " - The Email cannot be Empty";
                }
                if (!isValidEmail(player.Email))
                {
                    CamposInvalidos = true;
                    MensajeError = (string.IsNullOrEmpty(MensajeError)) ? "The Email is not valid" : MensajeError + " - The Email is not valid";
                }

                Player dbPlayer = _repository.FindByEmail(email);

                if (dbPlayer == null)
                {
                    CamposInvalidos = true;
                    MensajeError = "Email no registrado";
                }


                byte[] data = Encoding.ASCII.GetBytes(player.Password);
                data = new SHA256Managed().ComputeHash(data);
                string passwordHash = Encoding.ASCII.GetString(data);

                if (passwordHash != dbPlayer.Password)
                {
                    CamposInvalidos = true;
                    MensajeError = "Contraseña incorrecta";
                }


                if (!CamposInvalidos)
                {
                    Player newPlayer = new Player
                    {
                        Id = dbPlayer.Id,
                        Name = dbPlayer.Name,
                        Email = player.Email,
                        Password = dbPlayer.Password,
                        Avatar = dbPlayer.Avatar
                    };


                    var claims = new List<Claim>
                    {
                    new Claim("Player", newPlayer.Email),
                    new Claim("Avatar", dbPlayer.Avatar)
                    };

                    var claimsIdentity = new ClaimsIdentity(
                        claims, CookieAuthenticationDefaults.AuthenticationScheme
                    );

                    await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity)
                    );

                    _repository.Save(newPlayer);

                    return StatusCode(201, newPlayer);

                }
                else
                {
                    return StatusCode(403, MensajeError);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(403, ex.Message);
            }
        }

        [HttpPost("password")]
        public IActionResult ChangePassword([FromBody] PasswordPlayerDTO player)
        {
            try
            {
                string email = User.FindFirst("Player") != null ? User.FindFirst("Player").Value : "Guest";
                string MensajeError = "", mensajeErrorPassword = "";
                bool CamposInvalidos = false;
                if (string.IsNullOrEmpty(player.Password))
                {
                    CamposInvalidos = true;
                    MensajeError = (string.IsNullOrEmpty(MensajeError)) ? "The Password cannot be Empty" : MensajeError + " - The Password cannot be Empty";
                }

                if (player.NewPassword != player.NewPasswordRepeat)
                {
                    CamposInvalidos = true;
                    MensajeError = "La contraseña nueva debe coincidir";
                }

                if (!IsValidPassword(player.NewPassword, out mensajeErrorPassword))
                {
                    CamposInvalidos = true;
                    MensajeError = (string.IsNullOrEmpty(MensajeError)) ? mensajeErrorPassword : MensajeError + " - " + mensajeErrorPassword;
                }

                if (email == "Guest")
                {
                    CamposInvalidos = true;
                    MensajeError = "Usuario no registrado";
                }


                Player dbPlayer = _repository.FindByEmail(email);

                if (dbPlayer == null)
                {
                    CamposInvalidos = true;
                    MensajeError = "Usuario actual no encontrado";
                }


                byte[] data = Encoding.ASCII.GetBytes(player.Password);
                data = new SHA256Managed().ComputeHash(data);
                string passwordHash = Encoding.ASCII.GetString(data);

                if (dbPlayer.Password != passwordHash)
                {
                    CamposInvalidos = true;
                    MensajeError = "Contraseña incorrecta";
                }

                if (!CamposInvalidos)
                {
                    //Hash de la nueva password
                    byte[] data2 = Encoding.ASCII.GetBytes(player.NewPassword);
                    data2 = new SHA256Managed().ComputeHash(data2);
                    string newpasswordHash = Encoding.ASCII.GetString(data2);

                    Player newPlayer = new Player
                    {
                        Id = dbPlayer.Id,
                        Name = dbPlayer.Name,
                        Email = email,
                        Password = newpasswordHash,
                        Avatar = dbPlayer.Avatar
                    };

                    _repository.Save(newPlayer);

                    return StatusCode(201, newPlayer);
                }
                else
                {
                    return StatusCode(403, MensajeError);
                }

            }
            catch (Exception ex)
            {
                return StatusCode(403, ex.Message);
            }
        }

        [HttpPut("avatar")]
        public async Task<IActionResult> ChangeAvatar([FromBody] PlayerDTO player)
        {
            try
            {
                string email = User.FindFirst("Player") != null ? User.FindFirst("Player").Value : "Guest";

                if (email == "Guest")
                    return StatusCode(403, "Ocurrio un error");

                Player dbPlayer = _repository.FindByEmail(email);

                if (dbPlayer == null)
                    return StatusCode(403, "Ocurrio un error");


                Player newPlayer = new Player
                {
                    Id = dbPlayer.Id,
                    Name = dbPlayer.Name,
                    Email = email,
                    Password = dbPlayer.Password,
                    Avatar = player.Avatar
                };
                var claims = new List<Claim>
                {
                    new Claim("Avatar", newPlayer.Avatar),
                    new Claim("Player", email)
                };

                var claimsIdentity = new ClaimsIdentity(
                    claims, CookieAuthenticationDefaults.AuthenticationScheme
                );

                await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity)
                );

                _repository.Save(newPlayer);

                return StatusCode(201, newPlayer.Avatar);
            }
            catch (Exception ex)
            {
                return StatusCode(403, ex.Message);
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
        public bool IsValidPassword(string password, out string ErrorMessage)
        {
            var minLength = 8;
            var numUpper = 1;
            var numLower = 1;
            var numNumbers = 1;
            var numSpecial = 1;

            var upper = new Regex("[A-Z]");
            var lower = new Regex("[a-z]");
            var number = new Regex("[0-9]");
            var special = new Regex("[^a-zA-Z0-9]");

            if (password.Length < minLength)
            {
                ErrorMessage = "Password should not be less than or greater than 8 characters";
                return false;
            }
            if (upper.Matches(password).Count < numUpper)
            {
                ErrorMessage = "Password should contain at least one upper case letter";
                return false;
            }
            if (lower.Matches(password).Count < numLower)
            {
                ErrorMessage = "Password should contain at least one lower case letter";
                return false;
            }
            if (number.Matches(password).Count < numNumbers)
            {
                ErrorMessage = "Password should contain At least one numeric value";
                return false;
            }
            if (special.Matches(password).Count < numSpecial)
            {
                ErrorMessage = "Password should contain at least one special case character";
                return false;
            }

            ErrorMessage = string.Empty;
            return true;
        }
    }
}

