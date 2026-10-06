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

        public void ShowTransactionsByType(List<Block> chain, TransactionType? targetType = null)
        {
            var allTransactions = chain.SelectMany(b => b.Transactions).ToList();

            if (allTransactions.Count == 0)
            {
                Console.WriteLine("У блокчейні ще немає транзакцій.");
                return;
            }

            var typesToDisplay = targetType.HasValue
                ? new[] { targetType.Value }
                : Enum.GetValues(typeof(TransactionType)).Cast<TransactionType>();

            foreach (var type in typesToDisplay)
            {
                Console.WriteLine($"\n=== Список транзакцій типу {type} ===");
                var filtered = allTransactions.Where(t => t.Type == type).ToList();

                if (filtered.Count == 0)
                {
                    Console.WriteLine("Транзакції цього типу відсутні.");
                    continue;
                }

                foreach (var tx in filtered)
                {
                    Console.WriteLine($"[ID: {tx.Id}] From: {tx.From} | To: {tx.To} | Amount: {tx.Amount} | Type: {tx.Type}");
                }
            }
        }
    }
}