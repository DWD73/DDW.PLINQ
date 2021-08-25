﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace DDW.PLINQ
{
    class Program
    {
        static void Main(string[] args)
        {
            int countNumbers = 7000000;
            int index = 0;
            int sum = 0;
            int[] numbers = new int[countNumbers];

            Stopwatch sw = new Stopwatch();
            
            sw.Start();
            foreach (int t in Enumerable.Range(1, countNumbers))
                numbers[index++] = t;
            sw.Stop();

            Console.WriteLine($"Время создания массива из {countNumbers} элементов : {sw.Elapsed.Milliseconds} мс.\n");

            Console.WriteLine($"\nВыполнение суммирования массива обычным способом\n");
            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Start();
            Array.ForEach(numbers, delegate (int i) { sum += i; });
            stopwatch.Stop();

            GetResult(sum, stopwatch);


            var nChunks = 3;
            var totalLength = numbers.Count();
            var chunkLength = (int)Math.Ceiling(totalLength / (double)nChunks);

            var parts = Enumerable.Range(0, 3).Select(i => numbers.Skip(i * chunkLength).Take(chunkLength).ToArray()).ToArray();

            Console.WriteLine($"\nВыполнение суммирования массива с разбивкой его на равные части\n");
            Stopwatch stopwatch2 = new Stopwatch();

            stopwatch2.Start();

            for (int i = 0; i < nChunks; i++)
            {

                var thread = new Thread(DoWork);
                thread.IsBackground = true;
                thread.Start(parts[i]);
                thread.Join();
            }

            stopwatch2.Stop();

            GetResult(sum2, stopwatch2);

            Console.WriteLine($"\nВыполнение суммирования массива PLINQ способом\n");
            Stopwatch stopwatch3 = new Stopwatch();

            stopwatch3.Start();
            int sum3 = 0;
            object monitor = new object();
            //(numbers.AsParallel().Select(x => x)).ForAll(delegate (int i) { sum3 += i; });
            //sum3 = ParallelEnumerable.Range(0, countNumbers).Sum();
            //Parallel.For(0, countNumbers, i => { lock (monitor) sum3 += i; });
            //Parallel.For(0, countNumbers, i => { lock (monitor) sum3 += i; });
            sum3 = numbers.Aggregate((total, n) => total + n);
            stopwatch3.Stop();
            GetResult(sum3, stopwatch3);
        }

        private static void GetResult(int sum, Stopwatch stopwatch)
        {
            int time = stopwatch.Elapsed.Milliseconds;
            Console.WriteLine($"\tСумма чисел массива {sum}\n\tВремя выполнения : {time} мс.");
        }

        static int sum2 = 0;

        private static void DoWork(Object obj)
        {
            IEnumerable<int> _obj = (IEnumerable<int>)obj;

            Array.ForEach(_obj.ToArray(), delegate (int i) { sum2 += i; });
        }
    }
}
