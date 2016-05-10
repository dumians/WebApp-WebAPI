using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using System.Threading.Tasks;

namespace ConsoleTestApp
{
    class Program
    {
        static void Main(string[] args)
        {

            Sample.RunSample().Wait();

            Console.WriteLine("");

            Console.WriteLine("Done! Press the Enter key to Exit...");

            Console.ReadLine();

            return;
        }

        //static async Task Run()
        //{
        //    string baseAddress = "http://localhost:9090/";

        //    var provider = new TokenProvider(baseAddress);

        //    string _accessToken;

        //    Dictionary<string, string> _tokenDictionary;

        //    try
        //    {
        //        // Pass in the credentials and retrieve a token dictionary:

        //        _tokenDictionary = await provider.AcquireToken("manager@trivadis.com", "password");

        //        _accessToken = _tokenDictionary["access_token"];

        //        HttpClient client = new HttpClient();

        //        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

        //        var response = await client.GetAsync(baseAddress + "api/values");

        //        Console.WriteLine(await response.Content.ReadAsStringAsync());

        //        response = await client.GetAsync(baseAddress + "api/values");

        //        Console.WriteLine(await response.Content.ReadAsStringAsync());
        //    }
        //    catch (AggregateException ex)
        //    {
        //        // If it's an aggregate exception, an async error occurred:

        //        Console.WriteLine(ex.InnerExceptions[0].Message);
        //        Console.WriteLine("Press the Enter key to Exit...");
        //        Console.ReadLine();

        //        return;
        //    }

        //    catch (Exception ex)
        //    {

        //        // Something else happened:

        //        Console.WriteLine(ex.Message);

        //        Console.WriteLine("Press the Enter key to Exit...");

        //        Console.ReadLine();

        //        return;

        //    }


        //    foreach (var kvp in _tokenDictionary)
        //    {
        //        Console.WriteLine("{0}: {1}", kvp.Key, kvp.Value);

        //        Console.WriteLine("");
        //    }
        //}

    }
}
