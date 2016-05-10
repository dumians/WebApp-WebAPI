using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Microsoft.Azure.AppService
{
    public static class AppServiceClientExtensionMethods
    {
        static string aadInstance = "https://login.microsoftonline.com/{0}";
        /// <summary>
        /// Returns a AAD token to be used from a service application calling 
        /// a API App. 
        /// </summary>
        /// <param name="appServiceClient">Extended object</param>
        /// <param name="gatewayUri">The URI of the API App gateway</param>
        /// <param name="tenant">Your AAD tenant Eg yourTenant.onmicrosoft.com</param>
        /// <param name="clientId">The clientId is fetched from the AAD gateway identity blade in the Azure portal</param>
        /// <param name="appKey">The appKey is generated on your AAD application</param>
        /// <returns></returns>
        public static async Task<JObject> GetAadToken(this AppServiceClient appServiceClient, string gatewayUri, string tenant, string clientId, string appKey)
        {
            // Identity provider
            var authority = string.Format(CultureInfo.InvariantCulture, aadInstance, tenant);
            
            var authContext = new AuthenticationContext(authority);

            // The appIdUri is the identity of our AAD application
            var appIdUri = new Uri(new Uri(gatewayUri), "login/aad").ToString();

            // Get the clientId from the AAD gateway identity and
            // generate the appKey on the AAD tenant application 
            var credential = new ClientCredential(clientId, appKey);

            // Get the AAD token.
            AuthenticationResult result = authContext.AcquireToken(appIdUri, credential);

            // Wrap the token in a json object and return the token.
            var aadToken = new JObject {["access_token"] = result.AccessToken};
            return aadToken;

        }
    }
}