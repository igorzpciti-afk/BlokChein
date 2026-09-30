using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlokChein.Models
{
    public class Block
    {
        public int Index { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Data { get; set; }

        public string Hash { get; set; }

        public string PrevHash { get; set; }

        public string Author { get; set; }

        public long Nonce { get; set; } = 0;






    }
}
