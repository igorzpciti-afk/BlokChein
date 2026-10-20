using BlokChein.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlokChein.Services
{
    public class BalanceService
    {
        public decimal GetBalance(string address, List<Block> chain)
        {
            decimal balance = 0; // Ініціалізація балансу користувача. Початкове значення встановлюється в 0, оскільки на початку користувач не має коштів.
            foreach (var block in chain)
            {
                foreach (var tx in block.Transactions)
                {
                    // Перевірка, чи адреса користувача збігається з адресою відправника транзакції. Якщо так, то сума транзакції віднімається з балансу користувача.
                    if (tx.From == address)
                    {
                        balance -= tx.Amount;
                    }

                    // Перевірка, чи адреса користувача збігається з адресою отримувача транзакції. Якщо так, то сума транзакції додається до балансу користувача.
                    if (tx.To == address)
                    {
                        balance += tx.Amount;
                    }
                }
            }


            // Повернення остаточного балансу користувача після обробки всіх блоків та транзакцій у ланцюжку.
            return balance;
        }

        public bool HasSufficientBalance(Transaction transaction, List<Block> chain, List<Transaction> transactionPool)
        {
            decimal balance = GetBalance(transaction.From, chain);
            foreach (var tx in transactionPool)
            {
                // Перевірка, чи адреса користувача збігається з адресою відправника транзакції. Якщо так, то сума транзакції віднімається з балансу
                if (tx.From == transaction.From)
                {
                    balance -= tx.Amount + tx.Fee;
                }
                // Перевірка, чи адреса користувача збігається з адресою отримувача транзакції. Якщо так, то сума транзакції додається до балансу користувача
                if (tx.To == transaction.From)
                {
                    balance += tx.Amount;
                }
            }
            return balance >= transaction.Amount;
        }

        public decimal GetAvailableBalance(string address, List<Block> chain, List<Transaction> transactionPool)
        {
            decimal balance = GetBalance(address, chain);
            foreach (var tx in transactionPool)
            {
                if (tx.From == address)
                {
                    balance -= tx.Amount;
                }
                if (tx.To == address)
                {
                    balance += tx.Amount;
                }
            }
            return balance;
        }
    }
}
