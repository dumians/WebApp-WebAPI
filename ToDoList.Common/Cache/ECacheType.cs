namespace ToDoList.Common.Cache
{
    using System;
    using System.Reflection;

    /// <summary>
    /// <para>Enumeration of available cache types.</para>
    /// <para>Uses an Attribute and a corresponding Extension methods in combination to emulate Java enum behaviour.</para>
      /// </summary>
    public enum ECacheType
    {
        [ECacheTypeName("APPFABRIC")]
        AppFabric,

        [ECacheTypeName("INMEMORY")]
        Memory,
    }

    #region Implementation of the "Enum Pattern":

    /// <summary>
    /// Provides a name attribute for <see cref="ECacheType"/> enum values.
    /// </summary>
    class ECacheTypeNameAttribute : Attribute
    {
        /// <summary>
        /// The property storing the actual name value.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">The name value to set.</param>
        internal ECacheTypeNameAttribute(string name)
        {
            Name = name;
        }
    }

    /// <summary>
    /// Extendsion methods for enumeration <see cref="ECacheType"/>.
    /// </summary>
    public static class ECacheTypeExtensions
    {
        /// <summary>
        /// Gets the name value of the <see cref="ECacheTypeNameAttribute"/> placed on the given <see cref="ECacheType"/>.
        /// </summary>
        /// <param name="cacheType">The <see cref="ECacheType"/> for which to get the name value of its <see cref="ECacheTypeNameAttribute"/>.</param>
        /// <returns>The name value of the <see cref="ECacheTypeNameAttribute"/> placed on the given <see cref="ECacheType"/></returns>
        public static string GetName(this ECacheType cacheType)
        {
            var cacheTypeNameAttribute = GetECacheTypeNameAttribute(cacheType);

            return cacheTypeNameAttribute.Name;
        }

        /// <summary>
        /// Gets the <see cref="ECacheType"/> corrensponding to the given name value.
        /// </summary>
        /// <param name="cacheTypeName">The name for which to get its <see cref="ECacheType"/>.</param>
        /// <returns>The <see cref="ECacheType"/> corrensponding to the given name value, or <see cref="ECacheType.Memory"/> in case of an invalid/unknown name.</returns>
        public static ECacheType GetByName(string cacheTypeName)
        {
            var cacheTypes = Enum.GetValues(typeof(ECacheType));

            for (var index = 0; index < cacheTypes.Length; index++)
            {
                var currCacheType = (ECacheType)cacheTypes.GetValue(index);
                if (currCacheType.GetName().Equals(cacheTypeName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return currCacheType;
                }
            }

            return ECacheType.Memory;
        }

        /// <summary>
        /// Gets the <see cref="ECacheTypeNameAttribute"/> placed on the given <see cref="ECacheType"/>.
        /// </summary>
        /// <param name="cacheType">The <see cref="ECacheType"/> for which to get its <see cref="ECacheTypeNameAttribute"/>.</param>
        /// <returns>The <see cref="ECacheTypeNameAttribute"/> placed on the given <see cref="ECacheType"/>.</returns>
        private static ECacheTypeNameAttribute GetECacheTypeNameAttribute(ECacheType cacheType)
        {
            return (ECacheTypeNameAttribute)Attribute.GetCustomAttribute(GetMemberInfo(cacheType), typeof(ECacheTypeNameAttribute));
        }

        /// <summary>
        /// Gets the <see cref="MemberInfo"/> for the given <see cref="ECacheType"/>.
        /// </summary>
        /// <param name="cacheType">The <see cref="ECacheType"/> for which to get its <see cref="MemberInfo"/>.</param>
        /// <returns>The <see cref="MemberInfo"/> for the given <see cref="ECacheType"/>.</returns>
        private static MemberInfo GetMemberInfo(ECacheType cacheType)
        {
            return typeof(ECacheType).GetField(Enum.GetName(typeof(ECacheType), cacheType));
        }
    }

    #endregion
}