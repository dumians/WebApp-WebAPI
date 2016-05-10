using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Owin.Security.OAuth;

namespace ToDoList.Server.Cloud.AuthService.AuthService
{
    public class MyAuthProvider : OAuthAuthorizationServerProvider
    {

        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            var scope = context.Parameters["scope"];

            if (scope != null && scope == "ValuesController")
                await Task.FromResult(context.Validated());
            else
                context.SetError("Invalid scope.");
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            if (context.Password != "password")
            {
                context.SetError("invalid_grant", "The user name or password is incorrect.");

                context.Rejected();          
            }
          
            ClaimsIdentity identity = new ClaimsIdentity(context.Options.AuthenticationType);

            identity.AddClaim(new Claim("user_name", context.UserName));
            identity.AddClaim(new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", "trivadis\\jdumitru"));
            // Add a Role Claim:
            identity.AddClaim(new Claim(ClaimTypes.Role, "TestRole"));

            context.Validated(identity);
        }
    }

}


