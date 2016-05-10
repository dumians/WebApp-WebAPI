using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.Practices.Unity;

namespace ToDoList.Common
{
    /// <summary>
    /// Reflection Utilities
    /// </summary>
    public static class ReflectionUtils
    {

        #region PublicMethodes

        /// <summary>
        /// gets all assembly names ifrom teh current doamin base directory
        /// </summary>
        /// <returns>list of AssemblyNames</returns>
        public static List<AssemblyName> GetAppDomainAssemblyNames()
        {
            var result = new List<AssemblyName>();
            GetAppDomainAssemblies().ForEach(path => result.Add(AssemblyName.GetAssemblyName(path)));

            return result;
        }


        /// <summary>
        /// get attributes of a given type
        /// </summary>
        /// <typeparam name="T">return type</typeparam>
        /// <param name="memberInfo">meberInfo</param>
        /// <returns>a set of type T</returns>
        public static ISet<T> GetAttributes<T>(MemberInfo memberInfo) where T : Attribute
        {
            return GetAttributes<T>(memberInfo, false);
        }

        /// <summary>
        /// Returns Attributes of Member
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="memberInfo">Member</param>
        /// <param name="inherit">Consider inherited attributes</param>
        /// <returns>Attributes</returns>
        public static ISet<T> GetAttributes<T>(MemberInfo memberInfo, bool inherit) where T : Attribute
        {
            return new HashSet<T>((T[])memberInfo.GetCustomAttributes(typeof(T), inherit));
        }


        /// <summary>
        /// returns a type
        /// if not found in current assembly, searches in all assemblies in current assembly base domain
        /// </summary>
        /// <param name="typeName">full qualified type name</param>
        /// <returns>requested type</returns>
        public static Type GetTypeGlobal(string typeName)
        {
            Type result = null;

            //try normal getType
            result = Type.GetType(typeName);
            if (result == null)
            {
                //try GetType in loaded assemblies
                var loadedAssemblies = new List<Assembly>(AppDomain.CurrentDomain.GetAssemblies().ToArray());
                result = GetTypeFromLoadedAssemblies(typeName, loadedAssemblies);
                if (result == null)
                {
                    //load assemblies not yet loaded
                    var additionalLoadedAssemblies = LoadAssembliesFromBaseDirectory(loadedAssemblies);
                    result = GetTypeFromLoadedAssemblies(typeName, additionalLoadedAssemblies);
                }
            }

            return result;
        }

        public static IEnumerable<MemberInfo> GetReferenceMembers(Type type)
        {
            var fields = type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static |
                            BindingFlags.GetField | BindingFlags.GetProperty |
                            BindingFlags.DeclaredOnly).Where(m => m is FieldInfo || m is PropertyInfo).Where(m => !m.Name.Contains("<"));

            return fields;
        }

        public static IEnumerable<MemberInfo> GetDependencyFields(Type toType, IEnumerable<MemberInfo> referenceMembers)
        {
            var result = Enumerable.Empty<MemberInfo>();
            var ctors = toType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

            // classes with default ctor only OR a Dependency attribute on some member => Dependency injection
            var referenceMemberList = referenceMembers as IList<MemberInfo> ?? referenceMembers.ToList();
            if (!ctors.Any(c => c.GetParameters().Any()))
            {
                // instance referenceMembers having a DependencyAttribute
                result = referenceMemberList.Where(m => GetAttributes<DependencyAttribute>(m).Any());
            }
            else
            {
                // take ctor having an InjectionConstructor attribute or the one having the most parameters
                ConstructorInfo injectedCtor = ctors.FirstOrDefault(c => GetAttributes<InjectionConstructorAttribute>(c).Any()) ??
                                               ctors.OrderByDescending(c => c.GetParameters().Count()).FirstOrDefault();

                if (injectedCtor != default(ConstructorInfo))
                {
                    var ctorParamTypes = injectedCtor.GetParameters().Select(p => p.ParameterType).ToList();

                    result = referenceMemberList.Where(m => (m.MemberType == MemberTypes.Field && ctorParamTypes.Contains(((FieldInfo)m).FieldType)) ||
                        (m.MemberType == MemberTypes.Property && ctorParamTypes.Contains(((PropertyInfo)m).PropertyType)));

                    // join Dependency properties
                    result = result.Union(referenceMemberList.Where(m => GetAttributes<DependencyAttribute>(m).Any()));
                }
            }
            return result;
        }


        #endregion


        #region internal


        public static object GetValue(object obj, string propertyName)
        {
            MemberInfo[] miList = obj.GetType().GetMember(propertyName, MemberTypes.Property | MemberTypes.Field,
                                                          BindingFlags.Public | BindingFlags.NonPublic |
                                                          BindingFlags.Instance);
            if (miList.Length == 0)
            {
                return "";
            }
            MemberInfo mi = miList[0];


            if (mi.MemberType == MemberTypes.Field)
                return ((FieldInfo)mi).GetValue(obj);
            else
                return ((PropertyInfo)mi).GetValue(obj, null);
        }

        public static void SetValue(object obj, string propertyName, object value)
        {
            MemberInfo mi = obj.GetType().GetMember(propertyName, MemberTypes.Property | MemberTypes.Field,
                                                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                [0];
            if (mi.MemberType == MemberTypes.Field)
                ((FieldInfo)mi).SetValue(obj, value);
            else
                ((PropertyInfo)mi).SetValue(obj, value, null);
        }

        public static Dictionary<string, MemberInfo> GetEnumMembers(Type type)
        {
            bool hasDataContract = GetDataContractAttribute(type) != null;
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var members = new Dictionary<string, MemberInfo>(fields.Length);
            foreach (var memberInfo in fields)
            {
                if (hasDataContract)
                {
                    var enumAttribute = GetAttribute<EnumMemberAttribute>(memberInfo);
                    if (enumAttribute != null)
                    {
                        string enumName = enumAttribute.Value;
                        if (String.IsNullOrEmpty(enumName))
                            enumName = memberInfo.Name;
                        members.Add(enumName, memberInfo);
                    }
                }
                else
                {
                    members.Add(memberInfo.Name, memberInfo);
                }
            }
            return members;
        }

        internal static DataContractAttribute GetDataContractAttribute(Type type)
        {
            return GetAttribute<DataContractAttribute>(type);
        }

        internal static T GetAttribute<T>(MemberInfo memberInfo) where T : Attribute
        {
            T result = null;

            ISet<T> allAttributes = GetAttributes<T>(memberInfo);
            if (allAttributes.Count > 0)
            {
                result = allAttributes.GetEnumerator().Current;
            }

            return result;
        }


        private static IEnumerable<Assembly> LoadAssembliesFromBaseDirectory(IEnumerable<Assembly> alreadyLoadedAssemblies)
        {
            var assemblyNames = GetAppDomainAssemblyNames();
            // reduce by already loaded assemblies
            foreach (var assembly in alreadyLoadedAssemblies)
            {
                assemblyNames.Remove(assembly.GetName());
            }
            // load remaining assemblies

            return assemblyNames.Select(assName => AppDomain.CurrentDomain.Load(assName)).ToList();
        }

        private static List<string> GetAppDomainAssemblies()
        {
            return new List<string>(Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll").ToArray());
        }



        private static Type GetTypeFromLoadedAssemblies(string typeName, IEnumerable<Assembly> loadedAssemblies)
        {
            return loadedAssemblies.Select(assembly => assembly.GetType(typeName)).FirstOrDefault(result => result != null);
        }

        #endregion
    }
}


