using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using rnd = RandomMath.RandomMath;

namespace brok1.Services
{
    public class Pseudorandom
    {
        List<int> log;
        public int chance, secretChance;
        public int success, loss;
        private bool wasLost;
        private int rowLostCount;

        public Pseudorandom(int chance)
        {
            this.chance = chance;
            this.secretChance = this.chance * 9/10;
            log = new List<int>();
            success = 0;
            loss = 0;
        }
        public int GetRandom(int min, int max)
        {
            int num = rnd.GetRealRandomInt(min, max);
            return num;
        }
        private void addToList(int logInt)
        {
            if (log.Count >= chance)
            {
                for (int i = chance; i <= log.Count - 1; i++)
                {
                    log.RemoveAt(i);
                    Console.WriteLine($"removed at: {i}, {string.Join(" ", log)}");
                }
            }
            log.Insert(0, logInt);
            Console.WriteLine($"inserted at 0: {logInt}, {string.Join(" ", log)}");
        }
        public bool ProcessChance()
        {
            int num = GetRandom(0, 100);
            bool hasWon;
            if (log.Contains(num))
            {
                int count = 0;
                int chance1 = secretChance;
                for (int i = 0; i <= log.Count - 1; i++)
                {
                    if (num == log[i])
                    {
                        log.RemoveAt(i);
                        Console.WriteLine($"removed at index: {i}, {string.Join(" ", log)}");
                        count += 1;
                        chance1 -= Math.Abs((15 - i) * 2) / count;
                        Console.WriteLine($"current chance i={i}: {chance1}");
                    }
                }
                int num1 = GetRandom(0, 100);
                addToList(num1);
                if (100 / chance < rowLostCount)
                {
                    secretChance = chance + 2 * (rowLostCount - (100 / chance));
                }
                Console.WriteLine($"{num1}% and {chance1}%, secret: {secretChance}%");
                hasWon = num1 <= chance1;
                if (hasWon)
                {
                    success += 1;
                    wasLost = false;
                    rowLostCount = 0;
                    secretChance = chance * 9/10;
                }
                else
                {
                    if (wasLost)
                    {
                        rowLostCount += 1;
                    }
                    loss += 1;
                    wasLost = true;
                }
                return hasWon;
            }
            addToList(num);
            if (100 / chance < rowLostCount)
            {
                secretChance = chance + 2 * (rowLostCount - (100 / chance));
            }
                Console.WriteLine($"secret: {secretChance}%");
            hasWon = num <= secretChance;
            if (hasWon)
            {
                success += 1;
                wasLost = false;
                rowLostCount = 0;
                secretChance = chance * 9/10;
            }
            else
            {
                if (wasLost)
                {
                    rowLostCount += 1;
                }
                loss += 1;
                wasLost = true;
            }
            return hasWon;
        }
    }
}
