using System;
using UnityEditor.PackageManager.UI;
using UnityEditor.PackageManager.ValidationSuite;
using UnityEngine.UIElements;

namespace Unity.PackageManagerUI.Develop.Editor {
    /// <summary>
    /// Implement this interface to add a publish extension to the package manager UI.
    /// </summary>
    internal interface IPublishExtension
    {
        /// <summary>
        /// The name of the extension. For example "Tgz", "Asset Store", ...
        /// </summary>
        string Name { get; }

        /// <summary>
        /// When the publish button is pressed for this extension
        /// </summary>
        void OnPublish(IPackageVersion packageVersion);
    }
}