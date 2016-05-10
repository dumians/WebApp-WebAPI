// -----------------------------------------------------------------------
// <copyright file="TrustedCall.cs" company="">
// Copyright (c) . All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using ToDoList.Common;

namespace ToDoList.Server.Common.PermisionsChecker
{
    public sealed class TrustedCall : IDisposable
    {
        private readonly bool _rootScope = false;

        private static ITrustedScope TrustedScope
        {
            get
            {
                return DependencyFactory.Resolve<ITrustedScope>();
            }
        }


        public TrustedCall()
        {
            _rootScope = TrustedScope.Enter();
        }

        public static bool IsTrusted()
        {
            return TrustedScope.Trusted;
        }

        public void Dispose()
        {
            if (_rootScope)
            {
                TrustedScope.Exit();
            }
        }
    }
}
