using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncApiClient
{
    class Program
    {
        static void Main(string[] args)
        {
            // Adapted from https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/async/start-multiple-async-tasks-and-process-them-as-they-complete
            // Runs as a console app

            DoWork();

            Console.ReadLine();
        }

        public static async Task DoWork() {
            // Declare a System.Threading.CancellationTokenSource.  
            CancellationTokenSource cts;

            cts = new CancellationTokenSource();

            try
            {
                await AccessTheWebAsync(cts.Token);
                Console.WriteLine("\r\nDownloads complete.");
                Console.WriteLine("Press any key to exit ...");
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("\r\nDownloads canceled.\r\n");
            }
            catch (Exception)
            {
                Console.WriteLine("\r\nDownloads failed.\r\n");
            }

            cts = null;

 
        }

        public static async Task AccessTheWebAsync(CancellationToken ct)
        {
            HttpClient client = new HttpClient();

            // Make a list of web addresses.  
            List<string> urlList = SetUpURLList();

            // ***Create a query that, when executed, returns a collection of tasks.  
            IEnumerable<Task<DownloadContent>> downloadTasksQuery =
                from url in urlList select ProcessURL(url, client, ct);

            // ***Use ToList to execute the query and start the tasks.   
            List<Task<DownloadContent>> downloadTasks = downloadTasksQuery.ToList();

            // ***Add a loop to process the tasks one at a time until none remain.  
            while (downloadTasks.Count > 0)
            {
                // Identify the first task that completes.  
                Task<DownloadContent> firstFinishedTask = await Task.WhenAny(downloadTasks);

                // ***Remove the selected task from the list so that you don't  
                // process it more than once.  
                downloadTasks.Remove(firstFinishedTask);

                // Await the completed task.  
                DownloadContent content  = await firstFinishedTask;
                Console.WriteLine( String.Format("\r\nLength of the download:  {0} for URL {1}", content.contentLength, content.url) );
            }
        }

        private static List<string> SetUpURLList()
        {
            List<string> urls = new List<string>
            {
                "http://msdn.microsoft.com",
                "http://msdn.microsoft.com/library/windows/apps/br211380.aspx",
                "http://msdn.microsoft.com/library/hh290136.aspx",
                "http://msdn.microsoft.com/library/dd470362.aspx",
                "http://msdn.microsoft.com/library/aa578028.aspx",
                "http://msdn.microsoft.com/library/ms404677.aspx",
                "http://msdn.microsoft.com/library/ff730837.aspx"
            };
            return urls;
        }

        private static async Task<DownloadContent> ProcessURL(string url, HttpClient client, CancellationToken ct)
        {
            // GetAsync returns a Task<HttpResponseMessage>.   
            HttpResponseMessage response = await client.GetAsync(url, ct);

            // Retrieve the website contents from the HttpResponseMessage.  
            byte[] urlContents = await response.Content.ReadAsByteArrayAsync();

            return new DownloadContent
            {
                contentLength = urlContents.Length,
                url = url
            };
        }

        private class DownloadContent
        {
            public int contentLength { get; set; }
            public string url { get; set; }
        }

    }

}

