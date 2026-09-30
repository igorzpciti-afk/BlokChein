using BlokChein.Models;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlokChein.Services
{
    public class HashingService
    {
        public string ComputeHash(Block block)
        {
            string rawData = $"{block.Index}{block.Data}{block.Timestamp.ToString("o")}{block.PrevHash}{block.Author}{block.Nonce}";
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