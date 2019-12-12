using System;
using System.Collections.Generic;

namespace Unity.PackageManagerUI.Develop.Editor {
    internal static class PackageManagerDevelopExtensions
    {
        public static IPublishExtension GetPublishExtension(string name)
        {
            foreach (var extension in PackageManagerDevelopExtensions.PublishExtensions)
            {
                if (extension.Name == name)
                    return extension;
            }

            return null;
        }
        
        internal static List<IPublishExtension> PublishExtensions { get { return publishExtensions ?? (publishExtensions = new List<IPublishExtension>()); } }
        static List<IPublishExtension> publishExtensions;
    
        /// <summary>
        /// Registers a new Package Manager UI publish extension
        /// </summary>
        /// <param name="publishExtension">A Package Manager UI publish extension</param>
        public static void RegisterPublishExtension(IPublishExtension publishExtension)
        {
            if (publishExtension == null)
                return;

            PublishExtensions.Add(publishExtension);
        }
    }
}