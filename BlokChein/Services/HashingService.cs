using BlokChein.Models;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace BlokChein.Services
{
    public class HashingService
    {

        public string ComputeHash(Block block)
        {
            var transactionsHash = ComputeMerkleRoot(block.Transactions);
            block.MerkleRoot = transactionsHash;
            string rawData = $"{block.Index}{transactionsHash}{block.Timestamp}{block.PrevHash}{block.Nonce}{block.Difficulty}";
            return ComputeSha256(rawData);
        }
        private string ComputeSha256(string input)
        {
            var inputBytes = Encoding.UTF8.GetBytes(input);
            var hashBytes = SHA256.HashData(inputBytes);

            return Convert.ToHexString(hashBytes).ToLower();
        }
        public string ComputeMerkleRoot(List<Transaction> transactions)
        {
            if (transactions.Count == 0)
                return string.Empty;

            List<string> merkleLeaves = new List<string>();
            foreach (var tx in transactions)
            {
                merkleLeaves.Add(ComputeSha256(tx.ToRawString()));
            }

            while (merkleLeaves.Count > 1)
            {
                List<string> newLevel = new List<string>();
                for (int i = 0; i < merkleLeaves.Count; i += 2)
                {
                    if (i + 1 < merkleLeaves.Count)
                    {
                        string combinedHash = ComputeSha256(merkleLeaves[i] + merkleLeaves[i + 1]);
                        newLevel.Add(combinedHash);
                    }
                    else
                    {
                        // Якщо непарна кількість хешів, дублюємо останній хеш
                        newLevel.Add(merkleLeaves[i]);
                    }
                }
                merkleLeaves = newLevel;
            }
            return merkleLeaves[0];
        }
        public void PrintMerkleTree(List<Transaction> transactions)
        {
            if (transactions == null || transactions.Count == 0)
            {
                Console.WriteLine("Tree is empty.");
                return;
            }

            List<string> currentLevel = new List<string>();
            foreach (var tx in transactions)
            {
                currentLevel.Add(ComputeSha256(tx.ToRawString()));
            }

            int levelIndex = 0;

 
            while (currentLevel.Count > 1)
            {
                Console.WriteLine($"Level {levelIndex}:");
                foreach (var hash in currentLevel)
                {
                    Console.WriteLine(hash);
                }
                Console.WriteLine();

                List<string> nextLevel = new List<string>();
                for (int i = 0; i < currentLevel.Count; i += 2)
                {
                    if (i + 1 < currentLevel.Count)
                    {
                        string combined = ComputeSha256(currentLevel[i] + currentLevel[i + 1]);
                        nextLevel.Add(combined);
                    }
                    else
                    {
 
                        nextLevel.Add(currentLevel[i]);
                    }
                }

                currentLevel = nextLevel;
                levelIndex++;
            }

            Console.WriteLine(currentLevel[0]);
            Console.WriteLine();
            Console.WriteLine($"Merkle Root: {currentLevel[0]}");
        }
    }
}