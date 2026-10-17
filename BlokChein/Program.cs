using System;
using System.Collections.Generic;
using BlokChein.Models;
using BlokChein.Services;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var displayService = new DisplayService();
var testBlockChain = new BlockChainService(1, 10000, 2);
var transactionService = new TransactionService();
var walletService = new WalletService();
var balanceService = new BalanceService();



var aliceWallet = walletService.CreateWallet("Alice");
var bobWallet = walletService.CreateWallet("Bob");
var charlieWallet = walletService.CreateWallet("Charlie");
var daveWallet = walletService.CreateWallet("Dave");

displayService.ShowWallet(aliceWallet);
displayService.ShowWallet(bobWallet);
displayService.ShowWallet(charlieWallet);
displayService.ShowWallet(daveWallet);

while (true)
{
    Console.WriteLine("\n1. Mine Block");
    Console.WriteLine("2. Display Blockchain");
    Console.WriteLine("3. Validate Blockchain");
    Console.WriteLine("4. Show Wallet Balances");
    Console.WriteLine("5. AddTransaction");
    Console.WriteLine("6. Show Transaction Pool");
    Console.WriteLine("7. Validate Economy");
    Console.WriteLine("8. Tamper Economy (Break a Block)"); 
    Console.WriteLine("9. Exit");
    Console.Write("Your choice: ");

    string choice = Console.ReadLine();

    try
    {
        switch (choice)
        {
            case "1": 
                testBlockChain.MinePendingTransaction(aliceWallet.Address);
                Console.WriteLine("Block successfully mined and added to the blockchain!");
                break;

            case "2":
                displayService.ShowChain(testBlockChain.Chain);
                break;

            case "3":
                bool isValid = testBlockChain.IsValid();
                Console.WriteLine($"Blockchain is valid: {(isValid ? "true" : "false")}");
                break;

            case "4": 
                Console.WriteLine($"Alice Balance (Available): {balanceService.GetAvailableBalance(aliceWallet.Address, testBlockChain.Chain, testBlockChain.MemPool)}");
                Console.WriteLine($"Bob Balance (Available): {balanceService.GetAvailableBalance(bobWallet.Address, testBlockChain.Chain, testBlockChain.MemPool)}");
                Console.WriteLine($"Charlie Balance (Available): {balanceService.GetAvailableBalance(charlieWallet.Address, testBlockChain.Chain, testBlockChain.MemPool)}");
                Console.WriteLine($"Dave Balance (Available): {balanceService.GetAvailableBalance(daveWallet.Address, testBlockChain.Chain, testBlockChain.MemPool)}");
                break;

            case "5": 
                var tx1 = transactionService.CreateTransaction(aliceWallet, bobWallet.Address, 10);
                var tx2 = transactionService.CreateTransaction(bobWallet, charlieWallet.Address, 5);

                testBlockChain.AddTransaction(tx1);
                testBlockChain.AddTransaction(tx2);
                Console.WriteLine("Transactions processed for MemPool.");
                break;

            case "6": 
                displayService.ShowMemPool(testBlockChain.MemPool);
                break;
            case "7": 
                testBlockChain.ValidateEconomy();
                break;
            case "8":
                testBlockChain.TamperBlockData();
                break;

            case "9":
                return;

            default:
                Console.WriteLine("Invalid choice. Try again.");
                break;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
}