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
        private readonly WalletService _walletService = new WalletService();

        public Transaction CreateTransaction(Wallet sender, string to, decimal amount)
        {
            var tx = new Transaction(sender.Address, to, amount, sender.PublicKey);

            tx.Signature = sender.SignTransaction(tx);

            var (isValid, message) = ValidateTransaction(tx);
            if (!isValid)
            {
                throw new InvalidOperationException($"Invalid transaction: {message}");
            }

            return tx;
        }

        public (bool IsValid, string Message) ValidateTransaction(Transaction transaction)
        {
            if (transaction == null)
                return (false, "Transaction is null.");

            if (string.IsNullOrWhiteSpace(transaction.From))
                return (false, "Sender address is required.");

            if (string.IsNullOrWhiteSpace(transaction.To))
                return (false, "Recipient address is required.");

            if (transaction.Amount <= 0)
                return (false, "Transaction amount must be greater than zero.");

            bool signIsValid = _walletService.VerifyTransactionSignature(transaction, transaction.PublicKey);
            if (!signIsValid)
                return (false, "Invalid transaction signature.");

            return (true, "Transaction is valid.");
        }
    }
}