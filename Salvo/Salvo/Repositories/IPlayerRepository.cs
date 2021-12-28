using Salvo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Salvo.Repositories
{
    public interface IPlayerRepository : IRepositoryBase<Player>
    {
        Player FindByEmail(string email);
        void Save(Player player);
        void Update(Player player);
        Boolean ValidatePassword(string password, out string ErrorMessage);
    }
}
