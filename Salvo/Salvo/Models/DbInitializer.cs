using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Salvo.Models
{
    public static class DbInitializer
    {
        public static void Initialize(SalvoContext context)
        {
            if (!context.Players.Any())
            {
                var players = new Player[]
                {
                    new Player
                    {
                        Email = "j.bauer@ctu.gov",
                        Name = "Jack Bauer",
                        Password = "24"
                    },
                    new Player
                    {
                        Email = "c.obrian@ctu.gov",
                        Name = "Chloe O'Brian",
                        Password = "42"
                    },
                    new Player
                    {
                        Email="kim_bauer@gmail.com",
                        Name="Kim Bauer",
                        Password="kb"
                    },
                    new Player
                    {
                        Email="t.almeida@ctu.gov",
                        Name="Tony Almeida",
                        Password="mole"
                    }
                };
                
                foreach (Player player in players)
                {
                    context.Players.Add(player);
                }
                
                context.SaveChanges();
            }
        }
    }
}
