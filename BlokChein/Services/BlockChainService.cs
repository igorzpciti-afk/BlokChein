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
            var systemPublicKey = new byte[] { 0x00 };
            var fixedGenesisTime = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var genesisTransaction = new Transaction("SYSTEM", "Alice", 50m, 0m, systemPublicKey)
            {
                IsCoinbase = true,
                Id = "GENESIS_TX_FIXED_ID",
                Timestamp = fixedGenesisTime
            };

            var transactionsList = new List<Transaction> { genesisTransaction };

            var merkleRoot = _hashingService.ComputeMerkleRoot(transactionsList);

            var genesisBlock = new Block
            {
                Index = 0,
                Transactions = transactionsList,
                PrevHash = "0",
                Difficulty = Difficulty,
                Timestamp = fixedGenesisTime,
                MerkleRoot = merkleRoot,
                Nonce = 0 
            };

            _miningService.MineBlock(genesisBlock, Difficulty);
            Chain.Add(genesisBlock);
        }

        //public Block MinePendingTransaction(string minerAddress)
        //{
        //    var validTransactions = new List<Transaction>();
        //    var tempChain = new List<Block>(Chain);
        //    Block lastBlock = Chain[^1];
        //    var simulatedTransactions = new List<Transaction>();

        //    decimal totalFees = 0;

        //    foreach (var transaction in MemPool)
        //    {
        //        var (isValid, message) = _transactionService.ValidateTransactionWithBalance(transaction, Chain, simulatedTransactions);
        //        if (isValid)
        //        {
        //            validTransactions.Add(transaction);
        //            simulatedTransactions.Add(transaction);
        //            totalFees += transaction.Fee;
        //        }
        //        else
        //        {
        //            Console.WriteLine($"Skipping invalid transaction: {message}");
        //        }
        //    }

        //    int newBlockIndex = lastBlock.Index + 1;
        //    decimal currentReward = GetMiningReward(newBlockIndex);

        //    var coinbase = _transactionService.CreateCoinbaseTransaction(minerAddress, currentReward + totalFees);

        //    var newBlock = new Block()
        //    {
        //        Index = newBlockIndex,
        //        Transactions = validTransactions,
        //        PrevHash = lastBlock.Hash,
        //        Difficulty = Difficulty,
        //    };

        //    newBlock.Transactions.Insert(0, coinbase);

        //    newBlock.MerkleRoot = _hashingService.ComputeMerkleRoot(newBlock.Transactions);

        //    _miningService.MineBlock(newBlock, Difficulty);
        //    Chain.Add(newBlock);

        //    MemPool.Clear();

        //    if (newBlock.Index % _adjustmentInterval == 0)
        //    {
        //        AdjustDifficulty();
        //    }
        //    return newBlock;
        //}

        public Block MinePendingTransaction(string minerAddress)
        {
            MemPool.RemoveAll(t => (DateTime.UtcNow - t.Timestamp).TotalMinutes > 5);

            int acceptedCount = 0;
            int rejectedCount = 0;
            var validTransactions = new List<Transaction>();
            var simulatedTransactions = new List<Transaction>();
            decimal totalFees = 0;

            var sortedMemPool = MemPool
                .OrderByDescending(t => t.Fee)
                .ThenBy(t => t.Timestamp)
                .ToList();

            foreach (var transaction in sortedMemPool)
            {
                if (validTransactions.Count >= 5) break;

                bool alreadyConfirmed = Chain.SelectMany(b => b.Transactions).Any(t => t.Id == transaction.Id);
                bool isExpired = (DateTime.UtcNow - transaction.Timestamp).TotalMinutes > 5;
                var (isValid, message) = _transactionService.ValidateTransactionWithBalance(transaction, Chain, simulatedTransactions);

                if (!isValid || alreadyConfirmed || isExpired)
                {
                    rejectedCount++;
                    continue;
                }

                validTransactions.Add(transaction);
                simulatedTransactions.Add(transaction);
                totalFees += transaction.Fee;
                acceptedCount++;
            }

            Block lastBlock = Chain[^1];
            int newBlockIndex = lastBlock.Index + 1;
            decimal miningReward = GetMiningReward(newBlockIndex);
            decimal minerReceived = miningReward + totalFees;

            var coinbase = _transactionService.CreateCoinbaseTransaction(minerAddress, minerReceived);

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

            var minedIds = newBlock.Transactions.Select(t => t.Id).ToList();
            MemPool.RemoveAll(t => minedIds.Contains(t.Id));

            Console.WriteLine($"Прийнято транзакцій: {acceptedCount}");
            Console.WriteLine($"Відхилено транзакцій: {rejectedCount}");
            Console.WriteLine($"Total Fees: {totalFees}");
            Console.WriteLine($"Mining Reward: {miningReward}");
            Console.WriteLine($"Miner Received: {minerReceived}");
            Console.WriteLine($"Залишок Mempool: {MemPool.Count}");

            if (newBlock.Index % _adjustmentInterval == 0)
            {
                AdjustDifficulty();
            }

            return newBlock;
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
            // 1. Максимум 20 транзакцій
            if (MemPool.Count >= 20)
            {
                Console.WriteLine("Refused: Mempool full");
                return false;
            }

            // 2. Однаковий Transaction Id не приймати двічі
            if (MemPool.Any(t => t.Id == transaction.Id) || Chain.SelectMany(b => b.Transactions).Any(t => t.Id == transaction.Id))
            {
                Console.WriteLine("Refused: Duplicate");
                return false;
            }

            // 3. Час життя максимум 5 хвилин
            if ((DateTime.UtcNow - transaction.Timestamp).TotalMinutes > 5)
            {
                Console.WriteLine("Refused: Expired");
                return false;
            }

            // 4. Перевірка балансу
            var (isValid, message) = _transactionService.ValidateTransactionWithBalance(transaction, Chain, MemPool);
            if (!isValid)
            {
                Console.WriteLine("Refused: Insufficient balance");
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

        //public bool IsValid(List<Block> chain)
        //{
        //    for (int i = 1; i < Chain.Count; i++)
        //    {
        //        var currentBlock = Chain[i];
        //        var prevBlock = Chain[i - 1];

        //        string currentHash = _hashingService.ComputeHash(currentBlock);

        //        if (currentBlock.Hash != currentHash)
        //            return false;

        //        if (prevBlock.Hash != currentBlock.PrevHash)
        //            return false;

        //        var target = new string('0', currentBlock.Difficulty); 
        //        if (!currentBlock.Hash.StartsWith(target))
        //        {
        //            return false;
        //        }

        //        if (currentBlock.MerkleRoot != _hashingService.ComputeMerkleRoot(currentBlock.Transactions))
        //        {
        //            return false;
        //        }
        //    }

        //    return true;
        //}
        public bool IsValid(List<Block> chainToValidate)
        {
            if (chainToValidate == null || chainToValidate.Count == 0) return false;

            var genesis = chainToValidate[0];
            if (genesis.Index != 0 || genesis.PrevHash != "0")
            {
                Console.WriteLine("Validation failed: Invalid Genesis block.");
                return false;
            }

            var allTransactionIds = new HashSet<string>();

            for (int i = 0; i < chainToValidate.Count; i++)
            {
                var currentBlock = chainToValidate[i];

                // Перевірка унікальності Transaction Id
                foreach (var tx in currentBlock.Transactions)
                {
                    if (!allTransactionIds.Add(tx.Id))
                    {
                        Console.WriteLine($"Validation failed: Duplicate Transaction Id '{tx.Id}'.");
                        return false;
                    }
                }

                if (i > 0)
                {
                    var prevBlock = chainToValidate[i - 1];
                    if (currentBlock.PrevHash != prevBlock.Hash) return false;
                    if (currentBlock.Hash != _hashingService.ComputeHash(currentBlock)) return false;

                    var target = new string('0', currentBlock.Difficulty);
                    if (!currentBlock.Hash.StartsWith(target)) return false;
                }

                if (currentBlock.MerkleRoot != _hashingService.ComputeMerkleRoot(currentBlock.Transactions))
                    return false;
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

        public bool TryAddReceivedBlock(Block block)
        {
            var lastBlock = Chain[^1];
            if (block.PrevHash != lastBlock.Hash)
            {
                Console.WriteLine("Invalid block: Previous hash does not match.");
                return false;
            }
            if (block.Index != lastBlock.Index + 1)
            {
                Console.WriteLine("Invalid block: Index is not sequential.");
                return false;
            }
            if (block.Hash != _hashingService.ComputeHash(block))
            {
                Console.WriteLine("Invalid block: Hash is incorrect.");
                return false;
            }
            if (block.MerkleRoot != _hashingService.ComputeMerkleRoot(block.Transactions))
            {
                Console.WriteLine("Invalid block: Merkle root is incorrect.");
                return false;
            }
            if (!block.Hash.StartsWith(new string('0', block.Difficulty)))
            {
                Console.WriteLine("Invalid block: Hash does not meet difficulty requirement.");
                return false;
            }

            var transactionId = block.Transactions.Where(t => !t.IsCoinbase).Select(t => t.Id).ToList();
            MemPool.RemoveAll(t => transactionId.Contains(t.Id));

            Chain.Add(block);
            return true;
        }

        //public bool TryReplaceChain(List<Block> newChain)
        //{
        //    if (newChain.Count <= Chain.Count)
        //        return false;
        //    if (newChain[0].Hash != Chain[0].Hash)
        //        return false;
        //    if (!IsValid(newChain))
        //        return false;

        //    Chain = newChain;

        //    var transactionRemove = Chain.SelectMany(tr => tr.Transactions).Select(t => t.Id).ToHashSet();

        //    MemPool.RemoveAll(x => transactionRemove.Contains(x.Id));

        //    return true;
        //}

        public bool TryReplaceChain(List<Block> newChain)
        {
            // Прийняти Chain, лише якщо він валідний і довший за поточний
            if (newChain.Count <= Chain.Count)
            {
                Console.WriteLine("Sync failed: Received chain is not longer than current chain.");
                return false;
            }

            if (newChain[0].Hash != Chain[0].Hash || !IsValid(newChain))
            {
                Console.WriteLine("Sync failed: Received chain is invalid.");
                return false;
            }

            // Замінюємо ланцюжок
            Chain = newChain;

            // Очищення Mempool після синхронізації
            var confirmedIds = Chain.SelectMany(b => b.Transactions).Select(t => t.Id).ToHashSet();
            MemPool.RemoveAll(tx => confirmedIds.Contains(tx.Id) || (DateTime.UtcNow - tx.Timestamp).TotalMinutes > 5);

            var validRemainingMemPool = new List<Transaction>();
            foreach (var tx in MemPool)
            {
                var (isTxValid, _) = _transactionService.ValidateTransactionWithBalance(tx, Chain, validRemainingMemPool);
                if (isTxValid) validRemainingMemPool.Add(tx);
            }
            MemPool = validRemainingMemPool;

            Console.WriteLine("[SUCCESS] Chain replaced and Mempool updated.");
            return true;
        }
    }
}