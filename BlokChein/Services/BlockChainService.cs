using BlokChein.Services;



namespace BlokChein.Models
{
    public class BlockChainService
    {
        public List<Block> Chain { get; set; }
        public int Difficulty { get; set; } = 4;

        private readonly double _targetBlockTime; // в миллисекундах или секундах
        private readonly int _adjustmentInterval;
        private readonly MiningService _miningService;
        private readonly HashingService _hashingService;

        public BlockChainService(int difficulty, double targetBlockTime, int adjustmentInterval)
        {
            this.Difficulty = difficulty;
            this._targetBlockTime = targetBlockTime;
            this._adjustmentInterval = adjustmentInterval;

            _hashingService = new HashingService();
            _miningService = new MiningService();
            Chain = new List<Block>();

            AddGenesisBlock();
        }

        private void AddGenesisBlock()
        {
            var genesisBlock = new Block
            {
                Index = 0,
                Transactions = new List<Transaction>
                {
                    new Transaction("Alice", "Bob", 50m, TransactionType.Transfer)
                },
                PrevHash = "0",
                Difficulty = Difficulty,
                Timestamp = DateTime.UtcNow
            };

            _miningService.MineBlock(genesisBlock, Difficulty);
            Chain.Add(genesisBlock);
        }

        public void AddBlock(List<Transaction> transactions)
        {
            Block lastBlock = Chain[^1];

            var newBlock = new Block
            {
                Index = lastBlock.Index + 1,
                Transactions = transactions,
                PrevHash = lastBlock.Hash,
                Difficulty = Difficulty,
                Timestamp = DateTime.UtcNow
            };

            _miningService.MineBlock(newBlock, Difficulty);
            Chain.Add(newBlock);

        
            if (newBlock.Index % _adjustmentInterval == 0)
            {
                AdjustDifficulty();
            }
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

        public bool IsValid()
        {
            for (int i = 1; i < Chain.Count; i++)
            {
                var currentBlock = Chain[i];
                var prevBlock = Chain[i - 1];

                string currentHash = _hashingService.ComputeHash(currentBlock);
                if (currentBlock.Hash != currentHash)
                    return false;

                if (prevBlock.Hash != currentBlock.PrevHash)
                    return false;

                string target = new string('0', currentBlock.Difficulty);
                if (!currentBlock.Hash.StartsWith(target))
                    return false;
            }

            return true;
        }
    }
}