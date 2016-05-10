using System;
using System.Configuration;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace ConsoleTestApp
{
    public class Sample
    {
        private static string aadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
        private static string tenant = ConfigurationManager.AppSettings["ida:Tenant"];
        private static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private static string todoListResourceId = ConfigurationManager.AppSettings["todo:TodoListResourceId"];

        private static string authority = String.Format(CultureInfo.InvariantCulture, aadInstance, tenant);

        public async static Task RunSample()
        {
            try
            {
                var authContext = new AuthenticationContext(authority);
                var result = authContext.AcquireToken(todoListResourceId, clientId, new UserCredential("test@azuresamplead.onmicrosoft.com", "96ojxpiY"));

                HttpClient client = new HttpClient();

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);

                Uri redirectUri = new Uri(ConfigurationManager.AppSettings["ida:RedirectUri"]);
                var response = await client.GetAsync(redirectUri.AbsoluteUri + "api/values");

                Console.WriteLine(await response.Content.ReadAsStringAsync());

                response = await client.GetAsync(redirectUri.AbsoluteUri + "api/values");

                Console.WriteLine(await response.Content.ReadAsStringAsync());
            }
            catch (AggregateException ex)
            {
                // If it's an aggregate exception, an async error occurred:

                Console.WriteLine(ex.InnerExceptions[0].Message);
                Console.WriteLine("Press the Enter key to Exit...");
                Console.ReadLine();

                return;
            }

            catch (Exception ex)
            {

                // Something else happened:

                Console.WriteLine(ex.Message);

                Console.WriteLine("Press the Enter key to Exit...");

                Console.ReadLine();

                return;

            }
        }
    }
}
