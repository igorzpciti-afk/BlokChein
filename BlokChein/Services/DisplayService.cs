using BlokChein.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlokChein.Services
{
        public class DisplayService
        {
            public void ShowChain(List<Block> chain)
            {
                foreach (var block in chain)
                {
                    Console.WriteLine($"Index {block.Index}");
                    Console.WriteLine($"Data {block.Data}");
                    Console.WriteLine($"Timestamp {block.Timestamp.ToString("o")}");
                    Console.WriteLine($"Hash {block.Hash}");
                    Console.WriteLine($"Nonce {block.Nonce}");
                    Console.WriteLine($"PrevHash {block.PrevHash}");
                    Console.WriteLine($"Author {block.Author}");
                    Console.WriteLine("----------------------------------------");
                }
            }
        }
    }
