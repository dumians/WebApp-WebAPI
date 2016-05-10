using System;
using ToDoList.Server.AuthService.Owin;

namespace ToDoList.Server.Cloud.AuthService.AuthService
{  
    /// <summary>
    /// Options used for configuration of <see cref="SecManAuthorizationMiddleware"/>.
    /// </summary>
    public class SecManMdwOptions 
    {
        /// <summary>
        /// The application identifier. See for more information SecurityManager documentation.
        /// </summary>
       // public Guid ApplicationId { get; set; }


        /// <summary>
        /// The name of the claim, which will be used as a user name of Security Manager.
        /// This is typically name-claim ('http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'),
        /// but it can be used any other claim. For example, Authentication Server might issue claim
        /// with user's social number. In that case mathich username in SecurityManager would be social number
        /// instead of user name.
        /// </summary>
        /// <remarks>This claim MUST exist in the ClaimIdentity.</remarks>
        public string NameClaim { get; set; }


        /// <summary>
        /// Specifies how long the tokens will be cached. After the cache expires,
        /// the middleware will for next request contact SecurityManager to get roles for the user
        /// contained in the token. These roles will be sored in the cache and remains there for specified amount of time.
        /// </summary>
        public TimeSpan CacheInterval { get; set; }


        public IRoleProvider RoleProvider { get; set; }

        /// <summary>
        /// Creates options for configuration of SecManAuthorizationMiddleware.
        /// </summary>
        /// <param name="applicationId">The application identifier. See for more information SecurityManager documentation.</param>
        /// <param name="nameClaim">The name of claim which will be used for user matching in Security Manager.</param>
        /// <param name="cacheInterval">Specifies how long the tokens will be cached. Default value is 1 hour.</param>
        public SecManMdwOptions(Guid applicationId, string nameClaim, int cacheInterval = 3600000, IRoleProvider roleProvider = null)
        {
            this.NameClaim = nameClaim;
          //  this.ApplicationId = new Guid(applicationId);
            this.CacheInterval = TimeSpan.FromMilliseconds(cacheInterval);

            this.RoleProvider = roleProvider ?? new SecManAuthorizationModule() { ApplicationId = applicationId };
        }
    }
}
