using BlokChein.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlokChein.Services
{
    public class DisplayService
    {
        public void PrintTransactionHistory(List<Block> chain, Wallet wallet)
        {
            Console.Clear();
            Console.WriteLine($"=== ІСТОРІЯ ТРАНЗАКЦІЙ ДЛЯ ГАМАНЦЯ: {wallet.Name} ({wallet.Address}) ===\n");

            decimal totalReceived = 0;
            decimal totalSent = 0;
            int transactionCount = 0;

            foreach (var block in chain)
            {
                foreach (var tx in block.Transactions)
                {
                    if (tx.From == wallet.Address || tx.To == wallet.Address)
                    {
                        transactionCount++;
                        Console.WriteLine($"[Блок №{block.Index}]");

                        if (tx.IsCoinbase)
                        {
                            Console.WriteLine($"  Тип: [COINBASE ТРАНЗАКЦІЯ]");
                            Console.WriteLine($"  Майнер (Отримувач): {tx.To}");
                            Console.WriteLine($"  Винагорода: +{tx.Amount} монет");
                            totalReceived += tx.Amount;
                        }
                        else
                        {
                            Console.WriteLine($"  Від кого: {tx.From}");
                            Console.WriteLine($"  Кому: {tx.To}");
                            Console.WriteLine($"  Сума: {tx.Amount} монет");

                            if (tx.From == wallet.Address)
                            {
                                totalSent += tx.Amount;
                            }
                            if (tx.To == wallet.Address)
                            {
                                totalReceived += tx.Amount;
                            }
                        }

                        Console.WriteLine($"  Час: {tx.Timestamp}");
                        Console.WriteLine(new string('-', 40));
                    }
                }
            }

            if (transactionCount == 0)
            {
                Console.WriteLine("У цьому блокчейні ще немає транзакцій за участю цього гаманця.");
            }

            decimal currentBalance = totalReceived - totalSent;
            Console.WriteLine("\n=== ПІДСУМОК ===");
            Console.WriteLine($"Загальна сума отриманих монет: {totalReceived}");
            Console.WriteLine($"Загальна сума відправлених монет: {totalSent}");
            Console.WriteLine($"Поточний баланс гаманця: {currentBalance}");
        }

        public void PrintWalletStatement(List<Block> chain, Wallet wallet)
        {
            Console.Clear();
            Console.WriteLine($"=== ВИПИСКА ПО БАЛАНСУ ГАМАНЦЯ: {wallet.Name} ({wallet.Address}) ===\n");

            decimal runningBalance = 0;
            decimal totalReceived = 0;
            decimal totalSpent = 0;
            decimal totalMiningReward = 0;
            int participantTransactionsCount = 0;

            foreach (var block in chain)
            {
                bool blockInvolvesWallet = false;
                var blockLog = new StringBuilder();
                blockLog.AppendLine($"Block #{block.Index}");

                foreach (var tx in block.Transactions)
                {
                    if (tx.IsCoinbase && tx.To == wallet.Address)
                    {
                        blockInvolvesWallet = true;
                        participantTransactionsCount++;
                        runningBalance += tx.Amount;
                        totalReceived += tx.Amount;
                        totalMiningReward += tx.Amount;

                        blockLog.AppendLine($"  COINBASE +{tx.Amount}");
                    }
                    else if (!tx.IsCoinbase && (tx.From == wallet.Address || tx.To == wallet.Address))
                    {
                        blockInvolvesWallet = true;
                        participantTransactionsCount++;

                        if (tx.From == wallet.Address)
                        {
                            runningBalance -= tx.Amount;
                            totalSpent += tx.Amount;
                            blockLog.AppendLine($"  {wallet.Name} → {tx.To}: -{tx.Amount}");
                        }

                        if (tx.To == wallet.Address)
                        {
                            runningBalance += tx.Amount;
                            totalReceived += tx.Amount;
                            blockLog.AppendLine($"  {tx.From} → {wallet.Name}: +{tx.Amount}");
                        }
                    }
                }

                if (blockInvolvesWallet)
                {
                    Console.Write(blockLog.ToString());
                    Console.WriteLine($"  Баланс: {runningBalance}\n");
                }
            }

            if (participantTransactionsCount == 0)
            {
                Console.WriteLine("Цей гаманець ще не брав участі в жодній транзакції в цьому блокчейні.");
            }

            Console.WriteLine("========================================");
            Console.WriteLine("ПІДСУМКОВА СТАТИСТИКА:");
            Console.WriteLine($"  Початковий баланс: 0");
            Console.WriteLine($"  Скільки всього отримано: {totalReceived}");
            Console.WriteLine($"  Скільки всього витрачено: {totalSpent}");
            Console.WriteLine($"  Скільки отримано за майнінг: {totalMiningReward}");
            Console.WriteLine($"  Фінальний баланс: {runningBalance}");
            Console.WriteLine("========================================");
        }
        public void ShowChain(List<Block> chain)
        {
            foreach (var block in chain)
            {
                Console.WriteLine($"Index: {block.Index}");
                Console.WriteLine($"Timestamp: {block.Timestamp}");
                Console.WriteLine($"Hash: {block.Hash}");
                Console.WriteLine($"PrevHash: {block.PrevHash}");
                Console.WriteLine($"MerkleRoot: {block.MerkleRoot}");


                if (block.Transactions != null)
                {
                    Console.WriteLine("Transactions:");
                    foreach (var tx in block.Transactions)
                    {
                        Console.WriteLine($"  - From: {tx.From} | To: {tx.To} | Amount: {tx.Amount}");
                    }
                }
                Console.WriteLine("--------------------------------------------------");
            }
        }

        public void ShowWallet(Wallet w1)
        {
            Console.WriteLine($"Wallet Name: {w1.Name}");
            Console.WriteLine($"Wallet Address: {w1.Address}");
            Console.WriteLine($"Public Key: {Convert.ToBase64String(w1.PublicKey)}");
        }

        public void ShowMemPool(List<Transaction> memPool)
        {
            Console.WriteLine("Transaction Pool:");
            foreach (var tx in memPool)
            {
                Console.WriteLine($" - {tx.ToRawString()}");
            }
        }
    }
}