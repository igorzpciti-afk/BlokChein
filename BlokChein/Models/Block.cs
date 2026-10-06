using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlokChein.Models;

namespace BlokChein.Models
{
    public class Block
    {
        public int Index { get; set; }

        public DateTime Timestamp { get; set; }
        public List<Transaction> Transactions { get; set; } = new List<Transaction>();

        public string Hash { get; set; }

        public string PrevHash { get; set; }

        public long Nonce { get; set; } = 0;

        public int Difficulty { get; set; }

        public double MiningDuration { get; set; }
    }
}
