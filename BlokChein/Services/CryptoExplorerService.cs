using BlokChein.Models;


namespace BlokChein.Services
{
    public class CryptoExplorerService
    {
        // 1. Пошук транзакції за Id
        public void FindById(List<Block> chain, string id)
        {
            bool found = false;
            foreach (var block in chain)
            {
                foreach (var tx in block.Transactions)
                {
                    if (tx.Id.Equals(id, StringComparison.OrdinalIgnoreCase))
                    {
                        PrintTx(block.Index, tx);
                        found = true;
                    }
                }
            }

            if (!found)
            {
                Console.WriteLine("Транзакцію з таким Id не знайдено.");
            }
        }

        // 2. Всі транзакції певного користувача (і як відправника, і як отримувача)
        public void FindByUser(List<Block> chain, string user)
        {
            bool found = false;
            foreach (var block in chain)
            {
                foreach (var tx in block.Transactions)
                {
                    if (tx.From.Equals(user, StringComparison.OrdinalIgnoreCase) ||
                        tx.To.Equals(user, StringComparison.OrdinalIgnoreCase))
                    {
                        PrintTx(block.Index, tx);
                        found = true;
                    }
                }
            }

            if (!found)
            {
                Console.WriteLine($"Транзакцій для користувача '{user}' не знайдено.");
            }
        }

        // 3. Транзакції, сума яких більша за вказану
        public void FindGreaterThan(List<Block> chain, decimal minAmount)
        {
            bool found = false;
            foreach (var block in chain)
            {
                foreach (var tx in block.Transactions)
                {
                    if (tx.Amount > minAmount)
                    {
                        PrintTx(block.Index, tx);
                        found = true;
                    }
                }
            }

            if (!found)
            {
                Console.WriteLine($"Транзакцій із сумою понад {minAmount} не знайдено.");
            }
        }

        // 4. Пошук найбільшої транзакції у всьому блокчейні
        public void FindMaxTransaction(List<Block> chain)
        {
            Transaction maxTx = null;
            int maxBlockIndex = -1;

            foreach (var block in chain)
            {
                foreach (var tx in block.Transactions)
                {
                    if (maxTx == null || tx.Amount > maxTx.Amount)
                    {
                        maxTx = tx;
                        maxBlockIndex = block.Index;
                    }
                }
            }

            if (maxTx != null)
            {
                Console.WriteLine("Найбільша транзакція у блокчейні:");
                PrintTx(maxBlockIndex, maxTx);
            }
            else
            {
                Console.WriteLine("Блокчейн порожній або транзакції відсутні.");
            }
        }

        //фільтрація та вивід за типами ( Type)
        public void PrintTransactionsByType(List<Block> chain)
        {
            var allTransactions = chain.SelectMany(b => b.Transactions).ToList();

            foreach (TransactionType type in Enum.GetValues(typeof(TransactionType)))
            {
                Console.WriteLine($"\n=== Список транзакцій типу {type} ===");
                var filteredList = allTransactions.Where(t => t.Type == type).ToList();

                if (filteredList.Count == 0)
                {
                    Console.WriteLine("Транзакції цього типу відсутні.");
                    continue;
                }

                foreach (var tx in filteredList)
                {
                    Console.WriteLine($"From: {tx.From} | To: {tx.To} | Amount: {tx.Amount} | Type: {tx.Type}");
                }
            }
        }

        private void PrintTx(int blockIndex, Transaction tx)
        {
            Console.WriteLine($"[Блок #{blockIndex}] Id: {tx.Id} | Від: {tx.From} | До: {tx.To} | Сума: {tx.Amount}");
        }
    }
}