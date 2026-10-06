using BlokChein.Models;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlokChein.Models;
using System.Linq;

namespace BlokChein.Services
{
    public class HashingService
    {
    
        public string ComputeHash(Block block)
        {
            var transactionsRaw = string.Join("", block.Transactions.Select(t => t.ToRawString()));
            var transactionsHash = ComputeSha256(transactionsRaw);

            string rawData = $"{block.Index}{transactionsHash}{block.Timestamp}{block.PrevHash}{block.Nonce}{block.Difficulty}";
            return ComputeSha256(rawData);
        }

        private string ComputeSha256(string input)
        {
            var inputBytes = Encoding.UTF8.GetBytes(input);
            var hashBytes = SHA256.HashData(inputBytes);

            return Convert.ToHexString(hashBytes).ToLower();
        }
    }
}