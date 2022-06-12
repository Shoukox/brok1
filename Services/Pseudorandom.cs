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
        public int native_chance;
        public int chance, secretChance;
        public int success, loss;
        public int lossAfterWin;
        private int rowLostCount;
        private bool wasLost;
        private bool isTwisted;
        private int listSize = 50;

        public Pseudorandom(int chance)
        {
            native_chance = chance;
            this.chance = chance;
            this.secretChance = this.chance * 9 / 10;
            log = new List<int>();
            success = 0;
            loss = 0;
            lossAfterWin = 0;
        }
        private void _EditChance(int chance)
        {
            this.chance = chance;
            this.secretChance = this.chance * 9 / 10;
        }
        public void EditChance(int chance)
        {
            isTwisted = true;
            this.chance = chance;
            this.secretChance = this.chance * 9 / 10;
        }
        public int GetRandom(int min, int max)
        {
            int num = rnd.GetRealRandomInt(min, max);
            return num;
        }
        private void addToList(int logInt)
        {
            if (log.Count >= listSize)
            {
                for (int i = listSize; i <= log.Count - 1; i++)
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
            if (isTwisted)
            {
                isTwisted = false;
            }
            else
            {
                _EditChance(native_chance + ((lossAfterWin) / 10));
            }
            int num = GetRandom(0, 100);
            bool hasWon;
            while (true)
                if (log.Contains(num))
                {
                    int count = 0;
                    int chance1 = secretChance;
                    lock(log)
                    {
                        for (int i = log.Count - 1; i >= 0; i--)
                        {
                            if (num == log[i])
                            {
                                log.RemoveAt(i);
                                //Console.WriteLine($"removed at index: {i}, {string.Join(" ", log)}");
                                count += 1;
                            }
                        }
                    }
                    chance1 = 100 / count;

                    int num1 = GetRandom(0, 100);
                    addToList(num1);

                    if (!(num1 < chance1))
                    {
                        num = GetRandom(0, 100);
                        continue;
                    }
                }
                else break;
            addToList(num);
            if ((100 / chance) * 0.9 < rowLostCount)
            {
                secretChance = chance + 2 * (rowLostCount - (int)((100 / chance) * 0.9));
            }
            Console.WriteLine($"{num} and {chance}%, secret: {secretChance}%, {success} {loss}");
            hasWon = num <= secretChance;
            if (hasWon)
            {
                success += 1;
                lossAfterWin = 0;
                wasLost = false;
                rowLostCount = 0;
                EditChance(native_chance);
            }
            else
            {
                if (wasLost)
                {
                    rowLostCount += 1;
                }
                lossAfterWin += 1;
                loss += 1;
                wasLost = true;
            }
            return hasWon;
        }
        public string SaveData()
        {
            string result = "";
            result += string.Join(",", log) + "=";
            result += $"{native_chance},{chance},{secretChance}" + "=";
            result += $"{success},{loss},{lossAfterWin}" + "=";
            result += $"{rowLostCount}, {wasLost}";
            return result;
        }
        public void LoadData(string pseudorandomString)
        {
            string[] splittedData = pseudorandomString.Split("=");
            string[] spl0 = splittedData[0].Split(",");
            string[] spl1 = splittedData[1].Split(",");
            string[] spl2 = splittedData[2].Split(",");
            string[] spl3 = splittedData[3].Split(",");

            //0
            if (!int.TryParse(spl0[0], out _))
                log = new List<int>();
            else
                log = spl0.Select(m => int.Parse(m)).ToList();

            //1
            native_chance = int.Parse(spl1[0]);
            chance = int.Parse(spl1[1]);
            secretChance = int.Parse(spl1[2]);

            //2
            success = int.Parse(spl2[0]);
            loss = int.Parse(spl2[1]);
            lossAfterWin = int.Parse(spl2[2]);

            //3
            rowLostCount = int.Parse(spl3[0]);
            wasLost = bool.Parse(spl3[1]);
        }
    }
}
