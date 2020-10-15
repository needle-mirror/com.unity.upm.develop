using System.Collections.Generic;
using System.Linq;

namespace Unity.PackageManagerUI.Develop.Editor
{
    internal static class PackageManagerDevelopExtensions
    {
        internal static List<IPublishExtension> publishExtensions { get { return m_PublishExtensions ?? (m_PublishExtensions = new List<IPublishExtension>()); } }
        private static List<IPublishExtension> m_PublishExtensions;

        /// <summary>
        /// Returns Package Manager UI publish extension which match given name. If none returns null
        /// </summary>
        /// <param name="name">A Package Manager UI publish extension name to find</param>
        /// <returns></returns>
        public static IPublishExtension GetPublishExtension(string name)
        {
            return publishExtensions.FirstOrDefault(extension => extension.name == name);
        }

        /// <summary>
        /// Registers a new Package Manager UI publish extension
        /// </summary>
        /// <param name="publishExtension">A Package Manager UI publish extension</param>
        public static void RegisterPublishExtension(IPublishExtension publishExtension)
        {
            if (publishExtension == null)
                return;

            publishExtensions.Add(publishExtension);
        }
    }
}
