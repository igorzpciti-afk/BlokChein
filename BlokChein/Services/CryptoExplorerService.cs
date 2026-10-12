using BlokChein.Models;


namespace BlokChein.Services
{
    public class CryptoExplorerService
    {
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

        public void PrintTransactionsByType(List<Block> chain, decimal minAmount)
        {
            var allTransactions = chain.SelectMany(b => b.Transactions).ToList();
            var filteredList = allTransactions.Where(t => t.Amount >= minAmount).ToList();

            Console.WriteLine($"\n=== Транзакції з сумою >= {minAmount} ===");


                if (filteredList.Count == 0)
                {
                    Console.WriteLine("Транзакцій з такою сумою немає.");
                    return;
                }

                foreach (var tx in filteredList)
                {
                    Console.WriteLine($"ID: {tx.Id} | From: {tx.From} | To: {tx.To} | Amount: {tx.Amount}");
                }
            }
        
        private void PrintTx(int blockIndex, Transaction tx)
        {
            Console.WriteLine($"[Блок #{blockIndex}] Id: {tx.Id} | Від: {tx.From} | До: {tx.To} | Сума: {tx.Amount}");
        }
    }
}