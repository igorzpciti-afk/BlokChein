using BlokChein.Services;
using System;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var blockChain = new BlockChainService();
var displayService = new DisplayService();

while (true)
{
    Console.WriteLine("\n=== Меню ===");
    Console.WriteLine($"Поточне цільове ім'я: {blockChain.Name}");
    Console.WriteLine("1. Змінити цільове ім'я");
    Console.WriteLine("2. Додати блок");
    Console.WriteLine("3. Показати ланцюжок (Show Chain)");
    Console.WriteLine("4. Перевірити валідність (Check Validity)");
    Console.WriteLine("5. Змінити дані блоку (Підробити блок)");
    Console.WriteLine("6. Перемайнити ТІЛЬКИ змінений блок");
    Console.WriteLine("7. Відновити весь ланцюжок (Перемайнити всі блоки)");
    Console.WriteLine("8. Вихід");
    Console.Write("Ваш вибір: ");

    string choice = Console.ReadLine();

    if (choice == "1")
    {
        Console.Write("Введіть нове цільове ім'я: ");
        string newName = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(newName))
        {
            blockChain.Name = newName;
            Console.WriteLine($"Цільове ім'я оновлено на: {blockChain.Name}");
        }
    }
    else if (choice == "2")
    {
        Console.Write("Введіть дані для блоку: ");
        string data = Console.ReadLine();
        blockChain.AddBlock(data);
        Console.WriteLine("Блок успішно додано!");
    }
    else if (choice == "3")
    {
        displayService.ShowChain(blockChain.Chain);
    }
    else if (choice == "4")
    {
        // Крок 2: Перевіряємо валідність блокчейну
        bool isValid = blockChain.IsValid();
        Console.WriteLine($"\nСтатус валідності блокчейну -> {isValid}");
    }
    else if (choice == "5")
    {
        // Крок 1: Підробка даних в одному з блоків
        Console.Write($"Введіть індекс блоку для зміни (від 0 до {blockChain.Chain.Count - 1}): ");
        if (int.TryParse(Console.ReadLine(), out int index))
        {
            Console.Write("Введіть нові (фальшиві) дані: ");
            string fakeData = Console.ReadLine();
            blockChain.CorruptBlock(index, fakeData);

            Console.WriteLine($"\n[!] Дані блоку {index} змінено!");
            Console.WriteLine($"Статус валідності блокчейну: {blockChain.IsValid()} (має бути False)");
        }
    }
    else if (choice == "6")
    {
        // Крок 3: Спроба перемайнити лише один змінений блок
        Console.Write("Введіть індекс зміненого блоку для перемайнінгу ТІЛЬКИ його: ");
        if (int.TryParse(Console.ReadLine(), out int index))
        {
            var (attempts, time) = blockChain.RemineSingleBlock(index);
            Console.WriteLine($"\nБлок {index} перемайнено за {time.TotalMilliseconds:F2} мс (спроб Nonce: {attempts}).");
            Console.WriteLine($"Статус валідності блокчейну: {blockChain.IsValid()}");
            Console.WriteLine("[!] Ланцюжок все одно INVALID, тому що у наступних блоках не збігається PrevHash!");
        }
    }
    else if (choice == "7")
    {
        // Кроки 4 та 5: Повне відновлення ланцюжка з виводом часу та кількості спроб
        Console.Write("Введіть індекс, починаючи з якого відновити ланцюжок: ");
        if (int.TryParse(Console.ReadLine(), out int index))
        {
            Console.WriteLine("Відновлення ланцюжка...");
            var (totalAttempts, totalTime) = blockChain.RepairChainFrom(index);

            Console.WriteLine($"\n=== ВІДНОВЛЕННЯ ЗАВЕРШЕНО ===");
            Console.WriteLine($"Загальна кількість спроб (Nonce): {totalAttempts}");
            Console.WriteLine($"Загальний час майнінгу: {totalTime.TotalSeconds:F3} сек ({totalTime.TotalMilliseconds:F2} мс)");
            Console.WriteLine($"Статус валідності блокчейну: {blockChain.IsValid()} (має бути True)");
        }
    }
    else if (choice == "8")
    {
        break;
    }
}