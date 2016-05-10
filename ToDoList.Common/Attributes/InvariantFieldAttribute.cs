//-----------------------------------------------------------------------
// <copyright file="InvariantFieldAttribute.cs" company="">
// Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;

namespace ToDoList.Common.Attributes
{
    /// <summary>
    /// This attribute marks the decorated field as a field that will NOT be changed during runtime.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class InvariantFieldAttribute : Attribute
    {
        public string Description { get; private set; }

        public InvariantFieldAttribute() : this( "" )
        { }

        public InvariantFieldAttribute( string description )
        {
            Description = description;
        }
    }
}
