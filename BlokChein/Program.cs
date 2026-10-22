
using BlokChein.Models;
using BlokChein.Services;
using System.Text.Json;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var testBlockChain = new BlockChainService(1, 10000, 2);
var displayService = new DisplayService();
var transactionService = new TransactionService();
var walletService = new WalletService();
var balanceService = new BalanceService();

var aliceWallet = walletService.CreateWallet("Alice");
var bobWallet = walletService.CreateWallet("Bob");
var charlieWallet = walletService.CreateWallet("Charlie");
var daveWallet = walletService.CreateWallet("Dave");
int port = int.Parse(args[0]);

var nodes = args.Skip(1).Select(x => {
    var p = x.Split(":");
    return new Node(p[0], int.Parse(p[1]));
}).ToList();




var networkService = new NetworkService(port, nodes);
networkService.Start(async (type, message) =>
{
    Console.WriteLine($"Received message of type {type}: {message}");
    switch (type)
    {
        case "CHAIN":
            var chain = JsonSerializer.Deserialize<List<Block>>(message);
            if (chain != null)
            {
                var isreplace = testBlockChain.TryReplaceChain(chain);
                Console.WriteLine("IS NEW CHAIN isreplace");
            }
            break;
        case "GET_CHAIN":
            await networkService.BroadCastAsync("CHAIN", testBlockChain.Chain);
            break;
        case "Transaction":
            var transaction = JsonSerializer.Deserialize<Transaction>(message);
            if (transaction != null)
            {
                Console.WriteLine($"Received transaction from {transaction.From} to {transaction.To} for amount...");
                testBlockChain.AddTransaction(transaction);
            }
            break;
        case "Block":
            var block = JsonSerializer.Deserialize<Block>(message);
            if (block != null)
            {
                Console.WriteLine($"Received block with index {block.Index} and hash {block.Hash}");
                testBlockChain.TryAddReceivedBlock(block);
            }
            break;
    }
});


displayService.ShowWallet(aliceWallet);
displayService.ShowWallet(bobWallet);
displayService.ShowWallet(charlieWallet);
displayService.ShowWallet(daveWallet);

while (true)
{
    Console.WriteLine("\n1. Змайнити блок");
    Console.WriteLine("2. Показати блокчейн");
    Console.WriteLine("3. Перевірити валідність блокчейна");
    Console.WriteLine("4. Показати баланси гаманців");
    Console.WriteLine("5. Додати транзакцію");
    Console.WriteLine("6. Показати пул транзакцій");
    Console.WriteLine("7. Перевірити економіку");
    Console.WriteLine("8. Пошкодити економіку (зламати блок)");
    Console.WriteLine("9. Скасувати транзакцію з мемпулу");
    Console.WriteLine("10. Вивести дерево Меркла");
    Console.WriteLine("11. Перевірити нагороди майнерів (ValidateMiningRewards)");
    Console.WriteLine("12. Показати загальну кількість монет (GetTotalCoinsCreated)");
    Console.WriteLine("13. Підробити Coinbase у блоці #9 (TamperCoinbase)");
    Console.WriteLine("14. Синхронізація");
    Console.WriteLine("15. Тест дубліката транзакції (Duplicate)");
    Console.WriteLine("16. Вихід");
    Console.Write("Ваш вибір: ");

    string choice = Console.ReadLine();

    try
    {
        switch (choice)
        {
            case "1":
                var newBlock = testBlockChain.MinePendingTransaction(aliceWallet.Address);
                networkService.BroadCastAsync("Block", newBlock);
                 break;

            case "2":
                displayService.ShowChain(testBlockChain.Chain);
                break;

            case "3":
                bool isValid = testBlockChain.IsValid(testBlockChain.Chain);
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

                    var newTx = transactionService.CreateTransaction(senderWallet, recipientWallet.Address, amount, 0m);

                    // Викликаємо метод і перевіряємо результат (true/false)
                    bool isAdded = testBlockChain.AddTransaction(newTx);

                    if (isAdded)
                    {
                        Console.WriteLine($"[SUCCESS] Transaction added to MemPool: {senderWallet.Name} -> {recipientWallet.Name} ({amount} coins)");
                    }
                }
                else
                {
                    Console.WriteLine("Error: Invalid amount entered!");
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
                if (testBlockChain.Chain.Count > 0)
                {
                    foreach (var block in testBlockChain.Chain)
                    {
                        Console.WriteLine($"\n========================================");
                        Console.WriteLine($"--- Дерево Меркла для блоку {block.Index} ---");
                        Console.WriteLine($"========================================");
                        testBlockChain.PrintMerkleTree(block.Transactions);
                    }
                }
                else
                {
                    Console.WriteLine("Блокчейн порожній.");
                }
                break;

            case "11":
                testBlockChain.ValidateMiningRewards();
                break;

            case "12":
                decimal totalCoins = testBlockChain.GetTotalCoinsCreated();
                Console.WriteLine($"Загальна кількість створених нових монет (без комісій): {totalCoins}");
                break;

            case "13":
                testBlockChain.TamperCoinbase(9);
                break;
            case "14":
                await networkService.BroadCastAsync("GET_CHAIN", "");
                break;
            case "15":
                Console.WriteLine("\n--- Ручний тест дубліката ---");
                var duplicateTestTx = transactionService.CreateTransaction(aliceWallet, bobWallet.Address, 15m, 0m);

                Console.WriteLine("Додаємо першу транзакцію:");
                testBlockChain.AddTransaction(duplicateTestTx);

                Console.WriteLine("Спроба додати ту саму транзакцію вдруге:");
                testBlockChain.AddTransaction(duplicateTestTx);
                break;

            case "16":
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