namespace BlokChein.Models
{
    public class Transaction
    {
        private static int _idCounter = 1; // Автоматичний лічильник

        public string Id { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; }

        public Transaction(string from, string to, decimal amount)
        {
            Id = (_idCounter++).ToString(); // Призначить 1, 2, 3 і т.д.
            From = from;
            To = to;
            Amount = amount;
            Timestamp = DateTime.UtcNow;
        }

        public string ToRawString()
        {
            return $"{Id}{From}{To}{Amount}{Timestamp}";
        }
    }
}