using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;

namespace ToDoList.Server.Cloud.AuthService.AuthService
{
    public static class AppBuilderExtensions
    {
        /// <summary>
        /// Installs the <see cref="SecManAuthorizationMiddleware"/> in the OWIN pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="options"></param>
        public static void UseSecManAuthorizationMiddleware(
            this IAppBuilder app, SecManMdwOptions options = null)
        {
            if (options == null)
                throw new ArgumentException("SecManMdwOptions must be specified!");
            
            app.Use<SecManAuthorizationMiddleware>(options);
        }
    }

    /// <summary>
    /// Security Manager Authorization OWIN middleware.
    /// This middleware should be installed after one of authentication middlewares. Authentication middleware
    /// is responsible for authentication fo the user and for establishing f the user context. After the 
    /// context is established SecManAuthorizationMiddleware will grab the 'NameClaim' from claim identity (<see cref="SecManMdwOptions"/>)
    /// and will lookup the user which UserName=Claims(ClaimName).
    /// After the user is looked up, middleware will get user roles from SecurityManager Service and append them to the claims
    /// as additional roles. In other words, this middleware extends the list of claims with roles stored in security manager.
    /// Additionally middleware ensures that only authorized operations will be executed.
    /// Because this middleware extends the claim identity with additional roles, all common .NET role based security 
    /// will work as usual after the middleware has pased authorization.
    /// </summary>
    public class SecManAuthorizationMiddleware : OwinMiddleware
    {
        /// <summary>
        /// We store in owin pipeline under this key all required authorization options.
        /// </summary>
        internal const string OptionsKey = "SecManMdw.Options";

        private SecManMdwOptions m_Opts;

        private OwinMiddleware m_Next;

        /// <summary>
        /// Creates the middleware instance.
        /// </summary>
        /// <param name="next">Nex middleware in chain.</param>
        /// <param name="opts"></param>
        public SecManAuthorizationMiddleware(OwinMiddleware next, SecManMdwOptions opts)
            : base(next)
        {
            m_Opts = opts;
            m_Next = next;
        }

        /// <summary>
        /// Setups the authorization settings relevant for Security Manager.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task Invoke(IOwinContext context)
        {
            context.Environment.Add(OptionsKey, m_Opts);

            await m_Next.Invoke(context);
        }
    }
}
