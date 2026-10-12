using BlokChein.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BlokChein.Services
{
    public class DisplayService
    {
        public void ShowChain(List<Block> chain)
        {
            foreach (var block in chain)
            {
                Console.WriteLine($"Index: {block.Index}");
                Console.WriteLine($"Timestamp: {block.Timestamp}");
                Console.WriteLine($"Hash: {block.Hash}");
                Console.WriteLine($"PrevHash: {block.PrevHash}");
                if (block.Transactions != null)
                {
                    Console.WriteLine("Transactions:");
                    foreach (var tx in block.Transactions)
                    {
                        Console.WriteLine($"  - From: {tx.From} | To: {tx.To} | Amount: {tx.Amount}");
                    }
                }
                Console.WriteLine("--------------------------------------------------");
            }
        }

        public void ShowWallet(Wallet w1)
        {
            Console.WriteLine($"Wallet Name: {w1.Name}");
            Console.WriteLine($"Wallet Address: {w1.Address}");
            Console.WriteLine($"Public Key: {Convert.ToBase64String(w1.PublicKey)}");
        }
    }
}