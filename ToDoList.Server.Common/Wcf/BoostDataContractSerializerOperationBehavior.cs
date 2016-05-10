// -----------------------------------------------------------------------
// <copyright file="BoostDataContractSerializerOperationBehavior.cs" company="Trivadis AG">
// Copyright (c) Trivadis AG. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Common.Wcf
{
   
    using System.ServiceModel.Description;

    /// <summary>
    /// Increases maximum number of serialized items
    /// </summary>
    public class BoostDataContractSerializerOperationBehavior : DataContractSerializerOperationBehavior
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BoostDataContractSerializerOperationBehavior"/> class.
        /// </summary>
        /// <param name="operation">The operation description.</param>
        public BoostDataContractSerializerOperationBehavior(OperationDescription operation)
            : base(operation)
        {
            MaxItemsInObjectGraph = int.MaxValue;
        }
    }
}
