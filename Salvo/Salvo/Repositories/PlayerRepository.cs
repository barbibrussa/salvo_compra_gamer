using Salvo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Salvo.Repositories
{
    public class PlayerRepository : RepositoryBase<Player>, IPlayerRepository
    {
        public PlayerRepository(SalvoContext repositoryContext) : base(repositoryContext)
        {

        }
        public Player FindByEmail(string email)
        {
            return FindByCondition(player => player.Email == email).FirstOrDefault();
        }

        public void Save(Player player)
        {
            Create(player);
            SaveChanges();
        }

        public bool ValidatePassword(string password, out string ErrorMessage)
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