using System.ServiceModel;
using Microsoft.Practices.Unity;

namespace ToDoList.Common
{
    /// <summary>
    /// Child Container Extension for OperationContext
    /// </summary>
    public class ChildContainerContext : IExtension<OperationContext>
    {
        /// <summary>
        /// Current instance of ChildContainerContext
        /// </summary>
        public static ChildContainerContext Current
        {
            get
            {
                var context = OperationContext.Current.Extensions.Find<ChildContainerContext>();
                if (context == null)
                {
                    context = new ChildContainerContext();
                    OperationContext.Current.Extensions.Add(context);
                }
                return context;
            }
        }

        /// <summary>
        /// Unity Child Container
        /// </summary>
        public IUnityContainer ChildContainer { get; set; }

        /// <summary>
        /// Called by the OperationContext.
        /// </summary>
        /// <param name="owner">the context the extension is attached to</param>
        public void Attach(OperationContext owner)
        {
        }

        /// <summary>
        /// Called by the OperationContext.
        /// </summary>
        /// <param name="owner">the context the operation is detached from</param>
        public void Detach(OperationContext owner)
        {
        }
    }
}