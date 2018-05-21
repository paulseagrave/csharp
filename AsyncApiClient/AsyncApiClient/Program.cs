﻿using System;
 using System.Collections.Generic;
 using System.Linq;
 using System.Net.Http;
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
            IEnumerable<Task<RoleSkills>> downloadTasksQuery =
                from url in urlList select ProcessURL(url, client, ct);

            // ***Use ToList to execute the query and start the tasks.   
            List<Task<RoleSkills>> downloadTasks = downloadTasksQuery.ToList();

            // ***Add a loop to process the tasks one at a time until none remain.  
            while (downloadTasks.Count > 0)
            {
                // Identify the first task that completes.  
                Task<RoleSkills> firstFinishedTask = await Task.WhenAny(downloadTasks);
 
                // ***Remove the selected task from the list so that you don't  
                // process it more than once.  
                downloadTasks.Remove(firstFinishedTask);

                // Await the completed task.  
                RoleSkills content  = await firstFinishedTask;
                Console.WriteLine("Role: {0}", content.Role);
                Console.WriteLine("Skills: {0}\n\n", String.Join(",", content.Skills));
            }
        }

        private static List<string> SetUpURLList()
        {
            List<string> urls = new List<string>
            {
                "http://candidate.stream.jobsite.co.uk:20007/v1/role/skills?role=business%20analyst",
                "http://candidate.stream.jobsite.co.uk:20007/v1/role/skills?role=lorry%20driver",
                "http://candidate.stream.jobsite.co.uk:20007/v1/role/skills?role=project%20manager",
                "http://candidate.stream.jobsite.co.uk:20007/v1/role/skills?role=software%20developer"
            };
            return urls;
        }

        private static async Task<RoleSkills> ProcessURL(string url, HttpClient client, CancellationToken ct)
        {
            // GetAsync returns a Task<HttpResponseMessage>.   
            HttpResponseMessage response = await client.GetAsync(url, ct);

            // Retrieve the website contents from the HttpResponseMessage.  
            RoleSkills roleSkills = await response.Content.ReadAsAsync<RoleSkills>();
            return roleSkills;
        }



        private class RoleSkills
        {
            public string Role { get; set; }

            public String[] Skills { get; set; }
        }

    }

}

