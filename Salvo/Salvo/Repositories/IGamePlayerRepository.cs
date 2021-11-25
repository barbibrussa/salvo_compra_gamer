using Salvo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Salvo.Repositories
{
    public interface IGamePlayerRepository
    {
        public GamePlayer GetGamePlayerView(long idGamePlayer);

        void Save(GamePlayer gamePlayer);

        GamePlayer FindById(long id);
    }
}
