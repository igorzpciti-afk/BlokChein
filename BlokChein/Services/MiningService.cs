using BlokChein.Models;
using BlokChein.Services;
using System.Diagnostics;

namespace BlokChein.Services
{
    public class MiningService
    {
        private readonly HashingService _hashingService = new HashingService();

        public void MineBlock(Block block, int difficulty)
        {

            string target = new string('0', difficulty);
            var sw = Stopwatch.StartNew();

            while (true)
            {
                block.Hash = _hashingService.ComputeHash(block);

                if (block.Hash.StartsWith(target))
                {
                    sw.Stop();
                    block.MiningDuration = sw.Elapsed.TotalSeconds;
                    break;
                }

                block.Nonce++;

                if (block.Nonce % 100000 == 0)
                {
                    Console.WriteLine($"Mining in progress... Current Nonce: {block.Nonce:N0}, Current Hash: {block.Hash}");
                }
            }
        }
    }
}