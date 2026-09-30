using BlokChein.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlokChein.Services
{
    public class BlockChainService
    {
        public List<Block> Chain { get; set; }

        public readonly MiningService _miningService = new MiningService();
        private readonly HashingService _hashingService = new HashingService();

        // Тепер тут вказано "Igor"
        public string Name { get; set; } = "Igor";

        public BlockChainService()
        {
            Chain = new List<Block>();
            AddGenesisBlock();
        }

        private void AddGenesisBlock()
        {
            var genesis = new Block()
            {
                Data = "0",
                Index = 0,
                Timestamp = DateTime.Parse("1.01.1900"),
                PrevHash = "0",
                Author = "System"
            };

            _miningService.MineBlock(genesis, Name);
            Chain.Add(genesis);
        }

        public void AddBlock(string data, string author = "User")
        {
            var lastBlock = Chain[^1];
            var newBlock = new Block()
            {
                Index = lastBlock.Index + 1,
                Data = data,
                Timestamp = DateTime.UtcNow,
                PrevHash = lastBlock.Hash,
                Author = author
            };

            _miningService.MineBlock(newBlock, Name);
            Chain.Add(newBlock);
        }

        public void CorruptBlock(int index, string newData)
        {
            if (index >= 0 && index < Chain.Count)
            {
                Chain[index].Data = newData;
            }
        }

        public (long attempts, TimeSpan timeTaken) RemineSingleBlock(int index)
        {
            if (index < 0 || index >= Chain.Count) return (0, TimeSpan.Zero);

            Chain[index].Nonce = 0;
            return _miningService.MineBlock(Chain[index], Name);
        }

        public (long totalAttempts, TimeSpan totalTime) RepairChainFrom(int startIndex)
        {
            long totalAttempts = 0;
            TimeSpan totalTime = TimeSpan.Zero;

            for (int i = startIndex; i < Chain.Count; i++)
            {
                if (i > 0)
                {
                    Chain[i].PrevHash = Chain[i - 1].Hash;
                }

                Chain[i].Nonce = 0;
                var (attempts, timeTaken) = _miningService.MineBlock(Chain[i], Name);

                totalAttempts += attempts;
                totalTime += timeTaken;
            }

            return (totalAttempts, totalTime);
        }

        public bool IsValid()
        {
            string hexName = Convert.ToHexString(Encoding.UTF8.GetBytes(Name)).ToLower();
            string targetHex = hexName.Length >= 4 ? hexName[..4] : hexName;

            for (int i = 0; i < Chain.Count; i++) // Перевіряємо ВСІ блоки, включаючи Genesis (i = 0)
            {
                var currentBlock = Chain[i];

                // 1. Перевірка хешу самого блоку
                if (currentBlock.Hash != _hashingService.ComputeHash(currentBlock))
                {
                    return false;
                }

                // 2. Перевірка зв'язку з попереднім блоком
                if (i > 0)
                {
                    var prevBlock = Chain[i - 1];
                    if (prevBlock.Hash != currentBlock.PrevHash)
                    {
                        return false;
                    }
                }

                // 3. Перевірка наявності цільового паттерну в хеші
                if (!currentBlock.Hash.ToLower().Contains(targetHex))
                {
                    return false;
                }
            }

            return true;
        }
    }
}