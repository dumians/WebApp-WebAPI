// -----------------------------------------------------------------------
// <copyright file="BoostDataContractSerializerEndpointBehavior.cs" company="">
// 
// </copyright>
// -----------------------------------------------------------------------

namespace Common.Wcf
{
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;

    /// <summary>
    /// Data Contract behaviour
    /// (applies BoostDataContractSerializerOperationBehavior)
    /// </summary>
    sealed public class BoostDataContractSerializerEndpointBehavior : IEndpointBehavior
    {
        #region IEndpointBehavior Members
        void IEndpointBehavior.ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
        }

        void IEndpointBehavior.AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
            ApplyBoosting(endpoint);
        }

        void IEndpointBehavior.ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            ApplyBoosting(endpoint);
        }

        void IEndpointBehavior.Validate(ServiceEndpoint endpoint)
        {
        }
        #endregion

        void ApplyBoosting(ServiceEndpoint endpoint)
        {
            foreach (OperationDescription operation in endpoint.Contract.Operations)
            {
                DataContractSerializerOperationBehavior dcsob = operation.Behaviors.Find<DataContractSerializerOperationBehavior>();

                if (dcsob != null)
                {
                    operation.Behaviors.Remove(dcsob);

                    operation.Behaviors.Add(new BoostDataContractSerializerOperationBehavior(operation));
                }
            }
        }
    }
}
