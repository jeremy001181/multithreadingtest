using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace MultipleThreadingTest
{
    class Program
    {
        
        static ConcurrentQueue<int> queue = new ConcurrentQueue<int>();
        static int completed = 0;
        static readonly Random randomDelay = new Random();
        static Stopwatch watch = new Stopwatch();

        static void WriteLine(string text, params object[] obj)
        {
            var log = string.Format(text, obj);
            Console.WriteLine("[{0}] {1}", Thread.CurrentThread.ManagedThreadId, log);
        }

        static void Main(string[] args)
        {
            const int TOTAL = 10;
            for (int i = 1; i <= TOTAL; i++)
            {
                queue.Enqueue(i);
            }

            const int instances = 10;
            
            watch.Start();

            for (int i = 0; i < instances; i++)
            {
                DoWork();
            }

            while (true)
            {
                if (completed == TOTAL)
                {
                    watch.Stop();
                    WriteLine("Done, duration {0}s", watch.Elapsed.ToString("g"));
                    break;
                }
            }

            Console.ReadLine();
        }

        private static async Task DoWork()
        {
            if (queue.IsEmpty)
            {
                return;
            }
            
            int item;
            queue.TryDequeue(out item);

            await Task.Run(async () =>
            {
                //var task = LongRunningTask(item);
                //task.Wait();

                await LongRunningTask(item);
            }).ContinueWith(async _ =>
            {
                if (!queue.IsEmpty)
                {
                    await DoWork();
                }
                //var task = DoWork();
                //task.Wait();
                
            });
        }

        public static async Task LongRunningTask(object item)
        {
            WriteLine("- {0} LongRunningTask start", item);

            var second = randomDelay.Next(1, 3);
            await Task.Delay(second * 1000);

            Interlocked.Increment(ref completed);

            WriteLine("- {0} LongRunningTask end after delayed {1}s", item, second);
        }
    }
}
