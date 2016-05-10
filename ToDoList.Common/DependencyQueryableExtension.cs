using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Unity;

namespace ToDoList.Common
{
    /// <summary>
    /// QueryableContainerExtension
    /// </summary>
    public class QueryableContainerExtension : UnityContainerExtension
    {
        private List<RegisterEventArgs> Registrations = new List<RegisterEventArgs>();

        protected override void Initialize()
        {
            this.Context.Registering += this.Context_Registering;
            
        }

        void Context_Registering(object sender, RegisterEventArgs e)
        {
            this.Registrations.Add(e);
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TFrom"></typeparam>
        /// <typeparam name="TTo"></typeparam>
        /// <returns></returns>
        public bool IsTypeRegistered<TFrom, TTo>()
        {
            return this.Registrations.FirstOrDefault((e) => e.TypeFrom == typeof(TFrom) && e.TypeTo == typeof(TTo)) != null;  
        } 

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TFrom"></typeparam>
        /// <typeparam name="TTo"></typeparam>
        /// <returns></returns>
        public bool IsTypeRegisteredAsSingleton<TFrom, TTo>()    
        {     
            return this.Registrations.FirstOrDefault(  (e) => e.TypeFrom == typeof(TFrom)   && e.TypeTo == typeof(TTo)   && e.LifetimeManager is ContainerControlledLifetimeManager) != null;  
        } 
    }

}
