using System.Text;
using System;

namespace BlokChein.Models
{
    public class Transaction
    {
        public string Id { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public decimal Amount { get; set; } 

        public decimal Fee { get; set; }
        public DateTime Timestamp { get; set; }

        public byte[] Signature { get; set; }

        public bool IsCoinbase { get; set; }

        public byte[] PublicKey { get; set; }

        public Transaction(string from, string to, decimal amount,decimal fee, byte[] publicKey)
        {
            Id = Guid.NewGuid().ToString(); 
            From = from;
            To = to;
            Amount = amount;
            Fee = fee;
            Timestamp = DateTime.UtcNow; 
            PublicKey = publicKey;
            IsCoinbase = false;

        }

        public string ToRawString()
        {
            return $"{Id}\t{From}\t{To}\t{Amount}\t{Timestamp}\t{Fee}";
        }

        public byte[] GetDataToSign()
        {
            return Encoding.UTF8.GetBytes(ToRawString());
        }
    }
}