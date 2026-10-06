using System;
using System.Collections.Generic;
using BlokChein.Models;
using BlokChein.Services;


Console.OutputEncoding = System.Text.Encoding.UTF8;

var displayService = new DisplayService();
var testBlockChain = new BlockChainService(1, 10000, 2);
var transactionService = new TransactionService();
var explorer = new CryptoExplorerService();

var pendingTransactions = new List<Transaction>();

var b1Tx = new List<Transaction> { new("Alice", "Bob", 50m), new("Bob", "Charlie", 150m) };
var b2Tx = new List<Transaction> { new("Charlie", "Alice", 200m), new("Dave", "Bob", 30m) };
var b3Tx = new List<Transaction> { new("Alice", "Dave", 500m) };

testBlockChain.AddBlock(b1Tx);
testBlockChain.AddBlock(b2Tx);
testBlockChain.AddBlock(b3Tx);

while (true)
{
    Console.WriteLine($"\n--- МЕНЮ БЛОКЧЕЙНУ (У пулі: {pendingTransactions.Count}) ---");
    Console.WriteLine("1. Додати блок (Замайнити транзакції з пулу)");
    Console.WriteLine("2. Додати транзакцію");
    Console.WriteLine("3. Показати весь блокчейн");
    Console.WriteLine("4. Перевірити цілісність");
    Console.WriteLine("5. Пошук транзакцій (Crypto Explorer)");
    Console.WriteLine("6. Вихід");
    Console.Write("Ваш вибір: ");

    var choice = Console.ReadLine();

    switch (choice)
    {
        case "1":
            if (pendingTransactions.Count == 0)
            {
                Console.WriteLine("Пул порожній! Спочатку додайте транзакцію (пункт 2).");
                break;
            }

            testBlockChain.AddBlock(new List<Transaction>(pendingTransactions));
            pendingTransactions.Clear();
            Console.WriteLine("Блок успішно замайнено та додано!");
            break;

        case "2":
            try
            {
                Console.Write("Введіть відправника (From): ");
                string sender = Console.ReadLine() ?? "";

                Console.Write("Введіть отримувача (To): ");
                string recipient = Console.ReadLine() ?? "";

                Console.Write("Введіть суму (Amount): ");
                if (!decimal.TryParse(Console.ReadLine(), out decimal amount))
                {
                    Console.WriteLine("Некоректний формат суми!");
                    break;
                }

                var newTx = transactionService.CreateTransaction(sender, recipient, amount);
                pendingTransactions.Add(newTx);

                Console.WriteLine("Транзакцію успішно додано до пулу очікування!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка додавання транзакції: {ex.Message}");
            }
            break;

        case "3":
            displayService.ShowChain(testBlockChain.Chain);
            break;

        case "4":
            bool isValid = testBlockChain.IsValid();
            Console.WriteLine($"Стан блокчейну: {(isValid ? "ВАЛІДНИЙ" : "ПОШКОДЖЕНИЙ")}");
            break;

        case "5":
            Console.WriteLine("\n--- ПОШУК ТРАНЗАКЦІЙ (CRYPTO EXPLORER) ---");
            Console.WriteLine("a. Знайти транзакцію за Id");
            Console.WriteLine("b. Показати всі транзакції користувача");
            Console.WriteLine("c. Показати транзакції більші за суму");
            Console.WriteLine("d. Показати найбільшу транзакцію");
            Console.Write("Ваш вибір пошуку: ");

            var searchChoice = Console.ReadLine()?.ToLower();

            switch (searchChoice)
            {
                case "a":
                    Console.Write("Введіть Id транзакції: ");
                    string id = Console.ReadLine() ?? "";
                    explorer.FindById(testBlockChain.Chain, id);
                    break;

                case "b":
                    Console.Write("Введіть ім'я користувача: ");
                    string user = Console.ReadLine() ?? "";
                    explorer.FindByUser(testBlockChain.Chain, user);
                    break;

                case "c":
                    Console.Write("Введіть мінімальну суму: ");
                    if (decimal.TryParse(Console.ReadLine(), out decimal minAmount))
                    {
                        explorer.FindGreaterThan(testBlockChain.Chain, minAmount);
                    }
                    else
                    {
                        Console.WriteLine("Некоректний формат суми!");
                    }
                    break;

                case "d":
                    explorer.FindMaxTransaction(testBlockChain.Chain);
                    break;

                default:
                    Console.WriteLine("Невірний варіант пошуку.");
                    break;
            }
            break;

        case "6":
            return;

        default:
            Console.WriteLine("Невірний вибір. Спробуйте ще раз.");
            break;
    }
}