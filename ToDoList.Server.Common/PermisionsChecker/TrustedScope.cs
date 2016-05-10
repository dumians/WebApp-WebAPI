// -----------------------------------------------------------------------
// <copyright file="TrustedScope.cs" company="">
// Copyright (c) . All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace ToDoList.Server.Common.PermisionsChecker
{
    public class TrustedScope : ITrustedScope
    {
        public bool Trusted { get; private set; }

        public bool Enter()
        {
            var rootScope = !Trusted;

            Trusted = true;

            return rootScope;
        }

        public bool Exit()
        {
            var old = Trusted;

            Trusted = false;

            return old;
        }
    }
}
