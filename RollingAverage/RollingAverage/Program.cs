using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace RollingAverage
{
    class Program
    {
        static void Main(string[] args)
        {
            var stat = new StatHistory(
                new StatHistory.Level(12, 3),
                new StatHistory.Level(12, 4),
                new StatHistory.Level(12, 4));


            var rand = new Random();
            for (var i = 1; i < 1500; i++)
            {

                Console.Clear();

                Console.WriteLine(string.Format("average: {0}", stat.TotalAverage()));

                Console.WriteLine(string.Format("load average: {0}", String.Join(" ", stat.CurrentAverage())));

                foreach (var level in Enumerable.Range(0, stat.Levels))
                {
                    Console.Write(string.Format("current: {0,5:0.00} |", stat.LevelAverage(level)));
                    foreach (var x in stat.GetData(level))
                        Console.Write(string.Format("{0,5:0.0}", x));
                    Console.WriteLine();

                }
				var px = rand.Next(100) / 10.0;
                Console.WriteLine();
				Console.WriteLine(string.Format("next measurement: {0,5:0.00}", px));
				Console.WriteLine("anykey to add measurement");

                Console.ReadKey();
                stat.Put(px);
            }

            Console.ReadKey();
        }



    }
}
