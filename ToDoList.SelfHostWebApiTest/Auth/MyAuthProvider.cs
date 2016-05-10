using Microsoft.Owin.Security.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1.Auth
{
    public class MyAuthProvider : OAuthAuthorizationServerProvider
    {

        public override async Task ValidateClientAuthentication(

            OAuthValidateClientAuthenticationContext context)
        {

            // This call is required...

            // but we're not using client authentication, so validate and move on...

            await Task.FromResult(context.Validated());

        }

        public override async Task GrantResourceOwnerCredentials(

            OAuthGrantResourceOwnerCredentialsContext context)
        {

            // DEMO ONLY: Pretend we are doing some sort of REAL checking here:

            if (context.Password != "password")
            {

                context.SetError(

                    "invalid_grant", "The user name or password is incorrect.");

                context.Rejected();

                return;

            }
          
            ClaimsIdentity identity = new ClaimsIdentity(context.Options.AuthenticationType);

            identity.AddClaim(new Claim("user_name", context.UserName));
            identity.AddClaim(new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", "trivadis\\ddobric"));
            // Add a Role Claim:
            identity.AddClaim(new Claim(ClaimTypes.Role, "TestRole"));

            context.Validated(identity);
        }

    }

}


