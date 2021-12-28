using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Salvo.Models
{
    public class ResultsDTO
    {
        public int Wins { get; set; }
        public int Defeats { get; set; }
        public int Ties { get; set; }
        public int TotalGamesPlayed { get; set; }
    }
}
