using Microsoft.Owin.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1{
    class Program
    {
        static void Main(string[] args)
        {
            string baseAddress = "http://localhost:9090/";

            // Start OWIN host 
            using (WebApp.Start<Startup>(baseAddress))
            {
                // Create HttpCient and make a request to api/values 
                HttpResponseMessage response;
                using (HttpClient client = new HttpClient())
                {
                    response = client.GetAsync(baseAddress + "api/values").Result;
                }

                Console.WriteLine(response);
                Console.WriteLine(response.Content.ReadAsStringAsync().Result);

                Console.ReadLine();
            }



        }
    }
}
