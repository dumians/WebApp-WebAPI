using System;
using System.Collections.Generic;
using Microsoft.Practices.Unity;

namespace ToDoList.Common
{
    public interface IInterceptionBehaviourFactory
    {
        IEnumerable<InjectionMember> CreateInterceptionBehaviours(Type interfaceType);
    }
}
