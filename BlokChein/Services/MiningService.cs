using System;
using System.Diagnostics;
using System.Text;
using BlokChein.Models;

namespace BlokChein.Services
{
    public class MiningService
    {
        private readonly HashingService _hashingService;

        public MiningService()
        {
            _hashingService = new HashingService();
        }

        public (long attempts, TimeSpan timeTaken) MineBlock (Block block, string name)
        {
            string hexName = Convert.ToHexString(Encoding.UTF8.GetBytes(name)).ToLower();

            string targetHex = hexName.Length >= 4 ? hexName[..4] : hexName.Substring(0, 4);

            Stopwatch timer = Stopwatch.StartNew();

            long initialNonce = block.Nonce;


            Console.WriteLine($"Mining for '{name}' (Searching Hex pattern for letters: '{targetHex}')...");

            while (true)
            {
                block.Hash = _hashingService.ComputeHash(block);

                if (block.Hash.ToLower().Contains(targetHex))
                {
                    break;
                }

                block.Nonce++;

            }
                    timer.Stop();
                    long totalAttempts = block.Nonce - initialNonce + 1;
                    return (totalAttempts, timer.Elapsed);
                }
            }
        }