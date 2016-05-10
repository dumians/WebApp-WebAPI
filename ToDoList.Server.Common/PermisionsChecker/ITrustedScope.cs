// -----------------------------------------------------------------------
// <copyright file="ITrustedScope.cs" company="">
// Copyright (c) . All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace ToDoList.Server.Common.PermisionsChecker
{
    public interface ITrustedScope
    {
        bool Trusted { get; }

        bool Enter();

        bool Exit();
    }
}
