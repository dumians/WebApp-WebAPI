//using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using System.Net.Http;
//using System.Threading.Tasks;

//namespace ConsoleTestApp
//{
//    public class TokenProvider
//    {
//        string _hostUri;

//        public string AccessToken { get; private set; }

//        public TokenProvider(string hostUri)
//        {
//            _hostUri = hostUri;
//        }

//        public async Task<Dictionary<string, string>> AcquireToken(string userName, string password)
//        {
//            HttpResponseMessage response;

//            var pairs = new List<KeyValuePair<string, string>>
//                {
//                    new KeyValuePair<string, string>( "grant_type", "password" ), 
//                    new KeyValuePair<string, string>( "username", userName ), 
//                    new KeyValuePair<string, string> ( "password", password ),
//                    new KeyValuePair<string, string> ( "clientid", "id" ),
//                    new KeyValuePair<string, string> ( "scope", "ValuesController" ),
//                };

//            var content = new FormUrlEncodedContent(pairs);

//            using (var client = new HttpClient())
//            {
//                var tokenEndpoint = new Uri(new Uri(_hostUri), "Token");

//                response = await client.PostAsync(tokenEndpoint, content);
//            }

//            var responseContent = await response.Content.ReadAsStringAsync();

//            if (!response.IsSuccessStatusCode)
//            {
//                throw new Exception(string.Format("Error: {0}", responseContent));
//            }

//            return getTokenData(responseContent);
//        }


//        private Dictionary<string, string> getTokenData(string responseContent)
//        {
//            Dictionary<string, string> tokenDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseContent);

//            return tokenDictionary;
//        }
//    }
//}
