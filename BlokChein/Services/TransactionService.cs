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
        private readonly BalanceService _balanceService = new BalanceService();

        public Transaction CreateTransaction(Wallet sender, string to, decimal amount,decimal fee)
        {
            var tx = new Transaction(sender.Address, to, amount,fee, sender.PublicKey);

            tx.Signature = sender.SignTransaction(tx);

            var (isValid, message) = ValidateTransaction(tx);
            if (!isValid)
            {
                throw new InvalidOperationException($"Invalid transaction: {message}");
            }

            return tx;
        }

        public Transaction CreateCoinbaseTransaction(string minerAddress, decimal reward)
        {
            var coinbaseTx = new Transaction("Coinbase", minerAddress, reward,0, null)
            {
                IsCoinbase = true
            };
            return coinbaseTx;
        }

        public (bool IsValid, string Message) ValidateTransaction(Transaction transaction)
        {
            if (transaction == null)
                return (false, "Transaction is null.");

            if (transaction.IsCoinbase)
            {
                return (true, "Coinbase");
            }

            if (string.IsNullOrWhiteSpace(transaction.From))
                return (false, "Sender address is required.");

            if (string.IsNullOrWhiteSpace(transaction.To))
                return (false, "Recipient address is required.");

            if (transaction.Amount <= 0)
                return (false, "Transaction amount must be greater than zero.");

            if (transaction.Fee < 0)
                return (false, "Transaction fee cannot be negative.");

            bool signIsValid = _walletService.VerifyTransactionSignature(transaction, transaction.PublicKey);
            if (!signIsValid)
                return (false, "Invalid transaction signature.");

            return (true, "Transaction is valid.");
        }

        public (bool IsValid, string Message) ValidateTransactionWithBalance(Transaction transaction, List<Block> chain, List<Transaction> transactionPool)
        {
            var (isValid, message) = ValidateTransaction(transaction);
            if (!isValid)
            {
                return (false, message);
            }

            if (transaction.IsCoinbase)
            {
                return (true, "Coinbase is valid.");
            }

            bool hasSufficientBalance = _balanceService.HasSufficientBalance(transaction, chain, transactionPool);
            if (!hasSufficientBalance)
            {
                return (false, "Insufficient balance for the transaction.");
            }

            return (true, "Transaction is valid and has sufficient balance.");
        }
    }
}