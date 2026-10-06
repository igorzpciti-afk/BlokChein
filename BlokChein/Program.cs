using System;
using System.Collections.Generic;
using BlokChein;
using BlokChein.Models;
using BlokChein.Services;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var displayService = new DisplayService();
var testBlockChain = new BlockChainService(1, 10000, 2);
var transactionService = new TransactionService();
var explorer = new CryptoExplorerService();

var pendingTransactions = new List<Transaction>();

var b1Tx = new List<Transaction>
{
    new("Alice", "Bob", 50m, TransactionType.Transfer),
    new("Bob", "Charlie", 150m, TransactionType.Purchase)
};

var b2Tx = new List<Transaction>
{
    new("Charlie", "Alice", 200m, TransactionType.Gift),
    new("Dave", "Bob", 30m, TransactionType.Transfer)
};

var b3Tx = new List<Transaction>
{
    new("Alice", "Dave", 500m, TransactionType.Purchase)
};

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
    Console.WriteLine("6. Перегляд транзакцій за типами (Transfer, Purchase, Gift)");
    Console.WriteLine("7. Вихід");
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

                Console.WriteLine("Виберіть тип транзакції:");
                Console.WriteLine("1. Transfer");
                Console.WriteLine("2. Purchase");
                Console.WriteLine("3. Gift");
                Console.Write("Ваш вибір (1-3): ");

                string typeChoice = Console.ReadLine() ?? "";
                TransactionType type = typeChoice switch
                {
                    "2" => TransactionType.Purchase,
                    "3" => TransactionType.Gift,
                    _ => TransactionType.Transfer
                };

                var newTx = new Transaction(sender, recipient, amount, type);
                pendingTransactions.Add(newTx);

                Console.WriteLine($"Транзакцію [{type}] успішно додано до пулу очікування!");
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
            Console.WriteLine("\n--- ФІЛЬТРАЦІЯ ТРАНЗАКЦІЙ ЗА ТИПОМ ---");
            Console.WriteLine("1. Переглянути тільки Transfer");
            Console.WriteLine("2. Переглянути тільки Purchase");
            Console.WriteLine("3. Переглянути тільки Gift");
            Console.WriteLine("4. Вивести УСІ (згруповано за типами)");
            Console.Write("Ваш вибір (1-4): ");

            string filterChoice = Console.ReadLine() ?? "";

            switch (filterChoice)
            {
                case "1":
                    displayService.ShowTransactionsByType(testBlockChain.Chain, TransactionType.Transfer);
                    break;
                case "2":
                    displayService.ShowTransactionsByType(testBlockChain.Chain, TransactionType.Purchase);
                    break;
                case "3":
                    displayService.ShowTransactionsByType(testBlockChain.Chain, TransactionType.Gift);
                    break;
                case "4":
                    displayService.ShowTransactionsByType(testBlockChain.Chain, null);
                    break;
                default:
                    Console.WriteLine("Невірний вибір опції фільтрації.");
                    break;
            }
            break;

        case "7":
            return;

        default:
            Console.WriteLine("Невірний вибір. Спробуйте ще раз.");
            break;
    }
}