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
    Console.WriteLine("9. Cancel Transaction from Mempool");
    Console.WriteLine("10. Exit");
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
                var wallets = new Dictionary<string, Wallet>
                {
                    { "1", aliceWallet },
                    { "2", bobWallet },
                    { "3", charlieWallet },
                    { "4", daveWallet }
                };

                Console.WriteLine("\n--- Select Sender (From) ---");
                Console.WriteLine("1. Alice");
                Console.WriteLine("2. Bob");
                Console.WriteLine("3. Charlie");
                Console.WriteLine("4. Dave");
                Console.Write("Choice: ");
                string senderChoice = Console.ReadLine();

                Console.WriteLine("\n--- Select Recipient (To) ---");
                Console.WriteLine("1. Alice");
                Console.WriteLine("2. Bob");
                Console.WriteLine("3. Charlie");
                Console.WriteLine("4. Dave");
                Console.Write("Choice: ");
                string recipientChoice = Console.ReadLine();

                if (!wallets.ContainsKey(senderChoice) || !wallets.ContainsKey(recipientChoice))
                {
                    Console.WriteLine("Error: Invalid wallet selection!");
                    break;
                }

                if (senderChoice == recipientChoice)
                {
                    Console.WriteLine("Error: Sender and recipient cannot be the same person!");
                    break;
                }

                Console.Write("\nEnter amount to transfer: ");
                if (decimal.TryParse(Console.ReadLine(), out decimal amount) && amount > 0)
                {
                    var senderWallet = wallets[senderChoice];
                    var recipientWallet = wallets[recipientChoice];

                    var newTx = transactionService.CreateTransaction(senderWallet, recipientWallet.Address, amount);
                    testBlockChain.AddTransaction(newTx);

                    Console.WriteLine($"[SUCCESS] Transaction added to MemPool: {senderWallet.Name} -> {recipientWallet.Name} ({amount} coins)");
                }
                else
                {
                    Console.WriteLine("Error: Invalid amount enterred!");
                }
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
                Console.Write("Enter Transaction ID to cancel: ");
                string cancelTxId = Console.ReadLine();

                Console.Write("Enter your Wallet Address (who is trying to cancel): ");
                string senderAddress = Console.ReadLine();
                testBlockChain.CancelTransaction(cancelTxId, senderAddress);
                break;
            case "10":
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