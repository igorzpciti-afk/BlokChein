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
                Console.WriteLine($"Timestamp {block.Timestamp}");
                Console.WriteLine($"Hash {block.Hash}");
                Console.WriteLine($"Nonce {block.Nonce}");
                Console.WriteLine($"Mining Duration {block.MiningDuration}");
                Console.WriteLine($"Difficulty {block.Difficulty}");
                Console.WriteLine($"PrevHash {block.PrevHash}");
                if (block.Transactions != null && block.Transactions.Count > 0)
                {
                    Console.WriteLine("Transactions:");
                    foreach (var tx in block.Transactions)
                    {
                        Console.WriteLine($"  - {tx.ToRawString()}");
                    }
                }
                Console.WriteLine("--------------------------------------------------");
            }
        }
    }
}