using System;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.UI;
using UnityEngine;

namespace Unity.PackageManagerUI.Develop.Editor
{
    [InitializeOnLoad]
    internal class LocalPublishExtension : IPublishExtension
    {
        public string name => "Publish to disk";

        public void OnPublish(IPackageVersion packageVersion)
        {
            var destination = EditorUtility.SaveFolderPanel("Select package destination folder", "", "");
            if (string.IsNullOrEmpty(destination))
                return;

            new UpmPackOperation(packageVersion, destination,
                tarballPath => Debug.Log("Package saved successfully at: " + tarballPath),
                error => Debug.LogError("Error: " + error.message));
        }

        internal static void Publish(IPackageVersion packageVersion, string destination, Action<string> onSuccess, Action<Error> onError)
        {
            new UpmPackOperation(packageVersion, destination, onSuccess, onError);
        }

        static LocalPublishExtension()
        {
            PackageManagerDevelopExtensions.RegisterPublishExtension(new LocalPublishExtension());
        }
    }
}
