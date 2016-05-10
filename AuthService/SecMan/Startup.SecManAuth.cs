
using Microsoft.Owin;
using Microsoft.Owin.Security.OAuth;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Daenet.SecurityManager.Owin;
using System.Security.Claims;

// Added by Nu Get Package. 

namespace ToDoList.Server.Cloud.AuthService
{
  
    public partial class Startup
    {
        /// <summary>
        /// Use this method if you want quickly to activate mocked built in OAuth token provider.
        /// Later in production, you can exchange this provider aganist real one.
        /// This one is used for demo purposes only and enables you to quickly start with 
        /// Security Manager and custom authorization.
        /// </summary>
        /// <param name="appBuilder"></param>
        public void ConfigureOAuthAuthentication(IAppBuilder appBuilder)
        {
        
           var OAuthOptions = new OAuthAuthorizationServerOptions
            {
                TokenEndpointPath = new PathString("/Token"),
                Provider = new MockTokenProvider(),
                AccessTokenExpireTimeSpan = TimeSpan.FromDays(14),
                AllowInsecureHttp = true
            };


            appBuilder.UseOAuthAuthorizationServer(OAuthOptions);

            appBuilder.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());
        }
        
        
        /// <summary>
        /// Use this method to activate Security Manager authorization
        /// </summary>
        /// <param name="appBuilder"></param>
        public void ConfigureSecManAuthorization(IAppBuilder appBuilder)
        {
            //
            // OWIN Configuration for Security Manager middleware.
            appBuilder.UseSecManAuthorizationMiddleware(
                
                //
                // Here we provide required configuration. 
                new SecManMdwOptions(
                    // Put the application GUID here. Go to SecurityManager and lookup application there.
                    new Guid("8c76038a-d382-4efd-8adb-0c0c1b5c6158"),
 
                    // The name of claim (issued by authentication server), which
                    // which contains a matching username value as defined in SecurityManager system.
                    "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", 
                    
                    // Shows how to inject custom role provider.
                    // If you don't specify this, then built in provider will be used, which automatically connect to
                    // Security Manager web service to get roles of the user identified by claim name specified above.
                    roleProvider: new MockRoleProvider()));         
        }
    }
}
