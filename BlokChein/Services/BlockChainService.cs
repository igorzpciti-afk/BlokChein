using System;
using System.Collections.Generic;
using System.Linq;
using BlokChein.Services;

namespace BlokChein.Models
{
    public class BlockChainService
    {
        public List<Block> Chain { get; set; }
        public List<Transaction> MemPool { get; set; } = new List<Transaction>();
        public int Difficulty { get; set; } = 4;

        private int _halvingInterval = 4;
        private decimal _initialReward = 64m;

        private readonly double _targetBlockTime=10000;
        private readonly int _adjustmentInterval=2;
        private decimal _minimumReward = 2m;
        private readonly MiningService _miningService;
        private readonly HashingService _hashingService;
        private readonly TransactionService _transactionService;
        private readonly BalanceService _balanceService;

        public BlockChainService(int difficulty, double targetBlockTime, int adjustmentInterval)
        {
            this.Difficulty = difficulty;
            this._targetBlockTime = targetBlockTime;
            this._adjustmentInterval = adjustmentInterval;

            _hashingService = new HashingService();
            _miningService = new MiningService();
            _transactionService = new TransactionService();
            _balanceService = new BalanceService();

            Chain = new List<Block>();

            AddGenesisBlock();
        }

        private void AddGenesisBlock()
        {
            var merkleRoot = _hashingService.ComputeMerkleRoot(new List<Transaction>
            {
                new Transaction("SYSTEM", "Alice", 50m, 0m, new byte[] { 0x00 })
                {
                    IsCoinbase = true 
                }
            });
            byte[] systemPublicKey = new byte[] { 0x00 };

            var genesisBlock = new Block
            {
                Index = 0,
                Transactions = new List<Transaction>
                {
                    new Transaction("SYSTEM", "Alice", 50m, 0m, systemPublicKey)
                                {
                                    IsCoinbase = true 
                                }
                },
                PrevHash = "0",
                Difficulty = Difficulty,
                Timestamp = DateTime.UtcNow
            };

            _miningService.MineBlock(genesisBlock, Difficulty);
            Chain.Add(genesisBlock);
        }

        //public void AddBlock(List<Transaction> transactions, string minerAddress)
        //{
        //    // Перевірка кожної транзакції на валідність перед додаванням
        //    foreach (var transaction in transactions)
        //    {
        //        var (isValid, message) = _transactionService.ValidateTransactionWithBalance(transaction, Chain, transactions);
        //        if (!isValid)
        //        {
        //            continue;
        //        }
        //    }

        //    Block lastBlock = Chain[^1];

        //    var newBlock = new Block
        //    {
        //        Index = lastBlock.Index + 1,
        //        Transactions = transactions,
        //        PrevHash = lastBlock.Hash,
        //        Difficulty = Difficulty,
        //        Timestamp = DateTime.UtcNow
        //    };

        //    // Створення coinbase транзакції для винагороди майнера (наприклад, 50 монет)
        //    var coinbase = _transactionService.CreateCoinbaseTransaction(minerAddress, 50);
        //    newBlock.Transactions.Insert(0, coinbase); // Вставляємо на першу позицію в блоці

        //    _miningService.MineBlock(newBlock, Difficulty);
        //    Chain.Add(newBlock);

        //    if (newBlock.Index % _adjustmentInterval == 0)
        //    {
        //        AdjustDifficulty();
        //    }
        //}

        public void MinePendingTransaction(string minerAddress)
        {
            var validTransactions = new List<Transaction>();
            var tempChain = new List<Block>(Chain);
            Block lastBlock = Chain[^1];
            var simulatedTransactions = new List<Transaction>();

            decimal totalFees = 0;

            foreach (var transaction in MemPool)
            {
                var (isValid, message) = _transactionService.ValidateTransactionWithBalance(transaction, Chain, simulatedTransactions);
                if (isValid)
                {
                    validTransactions.Add(transaction);
                    simulatedTransactions.Add(transaction);
                    totalFees += transaction.Fee;
                }
                else
                {
                    Console.WriteLine($"Skipping invalid transaction: {message}");
                }
            }

            int newBlockIndex = lastBlock.Index + 1;
            decimal currentReward = GetMiningReward(newBlockIndex);

            var coinbase = _transactionService.CreateCoinbaseTransaction(minerAddress, currentReward + totalFees);

            var newBlock = new Block()
            {
                Index = newBlockIndex,
                Transactions = validTransactions,
                PrevHash = lastBlock.Hash,
                Difficulty = Difficulty,
            };

            newBlock.Transactions.Insert(0, coinbase);

            newBlock.MerkleRoot = _hashingService.ComputeMerkleRoot(newBlock.Transactions);

            _miningService.MineBlock(newBlock, Difficulty);
            Chain.Add(newBlock);

            MemPool.Clear();

            if (newBlock.Index % _adjustmentInterval == 0)
            {
                AdjustDifficulty();
            }
        }
        public Transaction CreateRewardTransaction(string minerAddress, decimal reward)
        {
            var rewardTx = new Transaction("RewardFee", minerAddress, reward, 0, null)
            {
                IsCoinbase = true
            };
            return rewardTx;
        }

        public bool AddTransaction(Transaction transaction)
        {
            var (isValid, message) = _transactionService.ValidateTransactionWithBalance(transaction, Chain, MemPool);
            if (!isValid)
            {
                Console.WriteLine($"Transaction is invalid: {message}");
                return false;
            }

            MemPool.Add(transaction);
            return true;
        }

        private void AdjustDifficulty()
        {
            var recentBlocks = Chain.Where(b => b.Index > 0).TakeLast(_adjustmentInterval).ToList();
            if (recentBlocks.Count == 0) return;

            var avgTime = recentBlocks.Average(b => b.MiningDuration);

            if (avgTime < _targetBlockTime)
            {
                Difficulty++;
                Console.WriteLine($"Difficulty increased to {Difficulty}");
            }
            else if (avgTime > _targetBlockTime)
            {
                Difficulty = Math.Max(1, Difficulty - 1);
                Console.WriteLine($"Difficulty decreased to {Difficulty}");
            }
        }

        public bool ValidateEconomy()
        {
            Dictionary<string, decimal> walletBalances = new Dictionary<string, decimal>();

            if (Chain.Count > 0)
            {
                var genesisBlock = Chain[0];
                foreach (var tx in genesisBlock.Transactions)
                {
                    if (tx.From == "SYSTEM")
                    {
                        if (!walletBalances.ContainsKey(tx.To))
                            walletBalances[tx.To] = 0;
                        walletBalances[tx.To] += tx.Amount;
                    }
                }
            }

            for (int i = 1; i < Chain.Count; i++)
            {
                var block = Chain[i];
                int coinbaseCount = 0;
                foreach (var tx in block.Transactions)
                {
                    if (tx.From == "Coinbase")
                    {
                        coinbaseCount++;

                        if (coinbaseCount > 1)
                        {
                            Console.WriteLine($"Block #{block.Index} INVALID");
                            Console.WriteLine($"Причина: Знайдено більше однієї coinbase-транзакції в блоці.");
                            return false;
                        }

                        decimal expectedReward = GetMiningReward(block.Index);

                        decimal blockFees = block.Transactions
                            .Where(t => !t.IsCoinbase && t.From != "Coinbase" && t.From != "SYSTEM")
                            .Sum(t => t.Fee);

                        decimal totalExpectedCoinbase = expectedReward + blockFees;

                        if (tx.Amount != totalExpectedCoinbase)
                        {
                            Console.WriteLine($"Блок #{block.Index}");
                            Console.WriteLine($"Очікувана нагорода: {expectedReward}");
                            Console.WriteLine($"Комісії: {blockFees}");
                            Console.WriteLine($"Очікуваний Coinbase: {totalExpectedCoinbase}");
                            Console.WriteLine($"Фактичний Coinbase: {tx.Amount}");
                            Console.WriteLine("НЕВАЛІДНО");
                            return false;
                        }


                        if (!walletBalances.ContainsKey(tx.To))
                            walletBalances[tx.To] = 0;

                        walletBalances[tx.To] += tx.Amount;
                    }
                }

                foreach (var tx in block.Transactions)
                {
                    if (tx.From != "Coinbase")
                    {
                        if (tx.From == "Coinbase")
                        {
                            Console.WriteLine($"Block #{block.Index} INVALID");
                            Console.WriteLine($"Причина: Звичайний користувач не може використовувати Coinbase для транзакцій.");
                            return false;
                        }

                        if (!walletBalances.ContainsKey(tx.From))
                            walletBalances[tx.From] = 0;
                        if (!walletBalances.ContainsKey(tx.To))
                            walletBalances[tx.To] = 0;

                        if (walletBalances[tx.From] < tx.Amount)
                        {
                            Console.WriteLine($"ПОМИЛКА в блоці #{block.Index}: Відправник '{tx.From}' намагається переказати {tx.Amount} для '{tx.To}', але його баланс = {walletBalances[tx.From]}");
                            return false;
                        }

                        walletBalances[tx.From] -= tx.Amount;
                        walletBalances[tx.To] += tx.Amount;

                        if (walletBalances[tx.From] < 0)
                        {
                            Console.WriteLine($"Block #{block.Index} INVALID");
                            Console.WriteLine($"Причина: Баланс гаманця {tx.From} став від'ємним.");
                            return false;
                        }
                    }
                }
            }

            Console.WriteLine("Blockchain economy is valid.");
            return true;
        }

        public bool IsValid()
        {
            for (int i = 1; i < Chain.Count; i++)
            {
                var currentBlock = Chain[i];
                var prevBlock = Chain[i - 1];

                // Обчислення хешу поточного блоку за допомогою сервісу хешування. Це дозволяє перевірити, чи дані в блоці не були змінені після його створення.
                string currentHash = _hashingService.ComputeHash(currentBlock);

                // Перевірка, чи обчислений хеш поточного блоку відповідає збереженому хешу в блоці. Якщо ні, блокчейн вважається недійсним.
                if (currentBlock.Hash != currentHash)
                    return false;

                // Перевірка, чи хеш попереднього блоку відповідає хешу, збереженому в поточному блоці. Якщо ні, блокчейн вважається недійсним.
                if (prevBlock.Hash != currentBlock.PrevHash)
                    return false;

                var target = new string('0', currentBlock.Difficulty); //"0000..."
                                                                       // Перевірка, чи хеш поточного блоку починається з потрібної кількості нулів (визначеної параметром difficulty). Якщо ні, блокчейн вважається недійсним.
                if (!currentBlock.Hash.StartsWith(target))
                {
                    return false;
                }

                // Merkle Root validation
                if (currentBlock.MerkleRoot != _hashingService.ComputeMerkleRoot(currentBlock.Transactions))
                {
                    return false;
                }
            }

            return true;
        }

        public decimal GetMiningReward(int blockIndex)
        {
            if (blockIndex <= 0) return _initialReward; 

            int halvings = (blockIndex - 1) / _halvingInterval;
            decimal reward = _initialReward / (decimal)Math.Pow(2, halvings);

            return Math.Max(reward, _minimumReward);
        }

        public bool ValidateMiningRewards()
        {
            for (int i = 1; i < Chain.Count; i++)
            {
                var block = Chain[i];

                var coinbaseTx = block.Transactions.FirstOrDefault(tx => tx.IsCoinbase || tx.From == "Coinbase" || tx.From == "SYSTEM");
                if (coinbaseTx == null)
                {
                    Console.WriteLine($"Block #{block.Index} INVALID: Відсутня Coinbase-транзакція.");
                    return false;
                }

                decimal fees = block.Transactions
                    .Where(tx => !tx.IsCoinbase && tx.From != "Coinbase" && tx.From != "SYSTEM")
                    .Sum(tx => tx.Fee);

                decimal expectedReward = GetMiningReward(block.Index);
                decimal expectedCoinbase = expectedReward + fees;

                if (coinbaseTx.Amount != expectedCoinbase)
                {
                    Console.WriteLine($"Block #{block.Index}");
                    Console.WriteLine($"Expected Reward: {expectedReward}");
                    Console.WriteLine($"Fees: {fees}");
                    Console.WriteLine($"Expected Coinbase: {expectedCoinbase}");
                    Console.WriteLine($"Actual Coinbase: {coinbaseTx.Amount}");
                    Console.WriteLine("INVALID");
                    return false;
                }
            }

            Console.WriteLine("Усі нагороди у блокчейні валідні.");
            return true;
        }

        public decimal GetTotalCoinsCreated()
        {
            decimal totalCoins = 0;

            foreach (var block in Chain)
            {
                totalCoins += GetMiningReward(block.Index);
            }

            return totalCoins;
        }

        public void TamperCoinbase(int blockIndex = 9)
        {
            var targetBlock = Chain.FirstOrDefault(b => b.Index == blockIndex);
            if (targetBlock != null)
            {
                var coinbaseTx = targetBlock.Transactions.FirstOrDefault(tx => tx.IsCoinbase || tx.From == "Coinbase" || tx.From == "SYSTEM");
                if (coinbaseTx != null)
                {
                    coinbaseTx.Amount += 100m;
                    Console.WriteLine($"[УСПІХ] Нагороду Coinbase у блоці #{targetBlock.Index} штучно змінено!");
                }
            }
            else
            {
                Console.WriteLine($"Блок #{blockIndex} не знайдено. Спочатку змайніть достатню кількість блоків.");
            }
        }

        public void TamperBlockData()
        {
            var targetBlock = Chain.FirstOrDefault(b => b.Index > 0 && b.Transactions.Any(tx => tx.From != "Coinbase" && tx.From != "SYSTEM"));

            if (targetBlock != null)
            {
                var normalTx = targetBlock.Transactions.FirstOrDefault(tx => tx.From != "Coinbase" && tx.From != "SYSTEM");

                if (normalTx != null)
                {
                    normalTx.Amount += 1000m; 
                    Console.WriteLine($"[УСПІХ] Дані в блоці #{targetBlock.Index} успішно пошкоджено! Суму транзакції змінено.");
                }
            }
            else
            {
                Console.WriteLine("Немає звичайних блоків з транзакціями для пошкодження. Додайте транзакцію через пункт 5 та змайніть блок через пункт 1!");
            }
        }

        public bool CancelTransaction(string transactionId, string senderAddress)
        {
            var transaction = MemPool.FirstOrDefault(tx => tx.Id == transactionId);

            if (transaction == null)
            {
                Console.WriteLine("Помилка: Транзакцію не знайдено в Mempool.");
                return false;
            }

            if (transaction.From != senderAddress)
            {
                Console.WriteLine("Помилка: Недостатньо прав для скасування цієї транзакції (адреса відправника не співпадає).");
                return false;
            }

            MemPool.Remove(transaction);
            Console.WriteLine($"[УСПІХ] Транзакцію {transactionId} успішно скасовано та видалено з Mempool.");
            return true;
        }

        public void PrintMerkleTree(List<Transaction> transactions)
        {
            _hashingService.PrintMerkleTree(transactions);
        }
    }
}