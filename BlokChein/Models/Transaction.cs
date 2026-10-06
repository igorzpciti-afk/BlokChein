namespace BlokChein.Models
{
    public enum TransactionType
    {
        Transfer,
        Purchase,
        Gift
    }

    public class Transaction
    {
        private static int _idCounter = 1;

        public string Id { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public decimal Amount { get; set; }
        public TransactionType Type { get; set; }
        public DateTime Timestamp { get; set; }

        public Transaction(string from, string to, decimal amount, TransactionType type)
        {
            Id = (_idCounter++).ToString();
            From = from;
            To = to;
            Amount = amount;
            Type = type;
            Timestamp = DateTime.UtcNow;
        }

        public Transaction(string from, string to, decimal amount, DateTime timestamp, TransactionType type)
            : this(from, to, amount, type)
        {
            Timestamp = timestamp;
        }

        public string ToRawString()
        {
            return $"[{Type}] Id: {Id} | Від: {From} | До: {To} | Сума: {Amount}";
        }
    }
}