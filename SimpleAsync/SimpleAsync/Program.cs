using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleAsync
{
    class Program
    {
        static void Main(string[] args)
        {
            DoWork();
        }

        private static async Task DoWork()
        {
            await GetAsyncResults();
            Console.WriteLine("Done.\nPress any key to exit . . .");
            Console.ReadLine();
        }

        private static async Task GetAsyncResults()
        {
            IEnumerable<Task<Results>> allTasks =
                from i in Enumerable.Range(1, 10) select GetAsyncTask(i);

            List<Task<Results>> taskList = allTasks.ToList();

            while (taskList.Count > 0)
            {
                Task<Results> finishedTask = await Task.WhenAny(taskList);

                taskList.Remove(finishedTask);
                Results result = await finishedTask;
                Console.WriteLine(result.Message);
            }
        }

        private static async Task<Results> GetAsyncTask(int i)
        {
            Random rand = new Random();
            int sleepMillis = rand.Next(1000);
            Thread.Sleep(sleepMillis);
            return new Results()
            {
                Index = i,
                Message = String.Format("{0} Slept for {1} milliseconds", i, sleepMillis)
            };
        }
    }

    internal class Results
    {
        public int Index { get; set; }
        public string Message { get; set; }
    }
}
