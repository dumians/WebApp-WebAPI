// -----------------------------------------------------------------------
// <copyright file="TransactionScopeAttribute.cs" company="">
// Copyright (c) . All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;

namespace ToDoList.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class SkipInterceptionAttribute : Attribute
    {
    }
}
