

using Microsoft.Owin;
using Microsoft.Owin.Security.OAuth;
using Owin;
using SelfHost;
using System;
using System.Web.Http;
using ToDoList.Server.Cloud.AuthService.AuthService;

namespace ConsoleApplication1
{
    //http://typecastexception.com/post/2015/01/19/ASPNET-Web-Api-Understanding-OWINKatana-AuthenticationAuthorization-Part-I-Concepts.aspx
    public class Startup
    {
        /// <summary>
        /// Use this method if you want quickly to activate mocked built in OAuth token provider.
        /// Later in production, you can exchange this provider aganist real one.
        /// This one is used for demo purposes only and enables you to quickly start with 
        /// Security Manager and custom authorization.
        /// </summary>
        /// <param name="appBuilder"></param>
        public void Configuration(IAppBuilder appBuilder)
        {
            // Configure Web API for self-host. 
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            var OAuthOptions = new OAuthAuthorizationServerOptions
            {
                TokenEndpointPath = new PathString("/Token"),
                Provider = new MockTokenProvider(),
                AccessTokenExpireTimeSpan = TimeSpan.FromDays(14),
                AllowInsecureHttp = true
            };


            appBuilder.UseOAuthAuthorizationServer(OAuthOptions);

            appBuilder.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());

            //
            // OWIN Configuration for Security Manager middleware.
            appBuilder.UseSecManAuthorizationMiddleware(

                //
                // Here we provide required configuration. 
                new SecManMdwOptions(
                // Application Id as defined in Security Manager system
                    new Guid("8c76038a-d382-4efd-8adb-0c0c1b5c6158"),

                    // The name of claim (issued by authentication server), which
                // which contains a matching username value as defined in SecurityManager system.
                    "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name",

                    // Shows how to inject custom role provider.
                // If you don't specify this, then built in provider will be used.
                    roleProvider: new MockRoleProvider()));

            appBuilder.UseWebApi(config);
        }
    }



}
