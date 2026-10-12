using System;
using System.Collections.Generic;
using BlokChein;
using BlokChein.Models;
using BlokChein.Services;

Console.OutputEncoding = System.Text.Encoding.UTF8;


        var displayService = new DisplayService();
        var testBlockChain = new BlockChainService(4, 10000, 2);
        var transactionService = new TransactionService();
        var walletService = new WalletService();

       
        var walletA = walletService.CreateWallet("Alice");
        var walletB = walletService.CreateWallet("Bob");

        Console.WriteLine($"Створено гаманець 1: {walletA.Name} (Адреса: {walletA.Address})");
        Console.WriteLine($"   -> Публічний ключ: {Convert.ToBase64String(walletA.PublicKey)}");
        Console.WriteLine($"Створено гаманець 2: {walletB.Name} (Адреса: {walletB.Address})");
        Console.WriteLine($"   -> Публічний ключ: {Convert.ToBase64String(walletB.PublicKey)}\n");

     
        // ВИКОНАННЯ ДОМАШНЬОГО ЗАВДАННЯ
        Console.WriteLine("=== ПЕРЕВІРКА ЦИФРОВИХ ПІДПИСІВ (ECDSA) ===");

        var testTx = transactionService.CreateTransaction(walletA, walletB.Address, 50m);
        Console.WriteLine($"[ТРАНЗАКЦІЯ] Від: {walletA.Name} -> До: {walletB.Name} | Сума: {testTx.Amount} монет");

        bool isValid1 = walletService.VerifyTransactionSignature(testTx, walletA.PublicKey);
        Console.WriteLine($"\n1. Перевірка правильного підпису (своїм ключем):");
        Console.WriteLine($"Використаний ключ: {Convert.ToBase64String(walletA.PublicKey)}");
        Console.WriteLine($"Результат:  {isValid1}");

        decimal originalAmount = testTx.Amount;
        testTx.Amount = 9999m; 
        bool isValid2 = walletService.VerifyTransactionSignature(testTx, walletA.PublicKey);
        Console.WriteLine($"\n2. Перевірка після зміни суми (з {originalAmount} на {testTx.Amount}):");
        Console.WriteLine($"Використаний ключ: {Convert.ToBase64String(walletA.PublicKey)}");
        Console.WriteLine($"Результат:  {isValid2}");
        testTx.Amount = originalAmount;

        bool isValid3 = walletService.VerifyTransactionSignature(testTx, walletB.PublicKey);
        Console.WriteLine($"\n3. Перевірка публічним ключем іншого гаманця (Bob):");
        Console.WriteLine($"Використаний ключ: {Convert.ToBase64String(walletB.PublicKey)}");
        Console.WriteLine($"Результат:  {isValid3}");
        Console.WriteLine("===========================================================\n");

        while (true)
        {
            Console.WriteLine("\n--- МЕНЮ БЛОКЧЕЙНУ ---");
            Console.WriteLine("1. Додати блок (з транзакціями)");
            Console.WriteLine("2. Показати весь блокчейн");
            Console.WriteLine("3. Перевірити цілісність");
            Console.WriteLine("4. Вихід");
            Console.Write("Ваш вибір: ");

            string choice = Console.ReadLine();

            var tx1 = transactionService.CreateTransaction(walletA, walletB.Address, 10);
            var tx2 = transactionService.CreateTransaction(walletB, walletA.Address, 5);

            switch (choice)
            {
                case "1":
                    testBlockChain.AddBlock(new List<Transaction> { tx1, tx2 });
                    break;
                case "2":
                    displayService.ShowChain(testBlockChain.Chain);
                    break;
                case "3":
                    bool isValid = testBlockChain.IsValid();
                    Console.WriteLine($"Блокчейн цілісний: {isValid}");
                    break;
                case "4":
                    return;
                default:
                    Console.WriteLine("Невірний вибір. Спробуйте ще раз.");
                    break;
            }
        }
