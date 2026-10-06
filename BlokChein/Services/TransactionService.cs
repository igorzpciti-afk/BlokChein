using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlokChein.Models;

namespace BlokChein.Services
{
    public class TransactionService
    {
        public Transaction CreateTransaction(string from, string to, decimal amount)
        {
            var tx = new Transaction(from, to, amount);
            var (isValid, message) = ValidateTransaction(tx);
            if (!isValid)
            {
                throw new InvalidOperationException($"Invalid transaction: {message}");
            }

            return tx;
        }

        public (bool isValid, string message) ValidateTransaction(Transaction transaction)
        {
            if (transaction == null)
            {
                return (false, "Transaction is null.");
            }

            if (string.IsNullOrWhiteSpace(transaction.From))
            {
                return (false, "Sender address is required.");
            }

            if (string.IsNullOrWhiteSpace(transaction.To))
            {
                return (false, "Recipient address is required.");
            }

            if (transaction.Amount <= 0)
            {
                return (false, "Transaction amount must be greater than zero.");
            }

            return (true, "Transaction is valid.");
        }
    }
}