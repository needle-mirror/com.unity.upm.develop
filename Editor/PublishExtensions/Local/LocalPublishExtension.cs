using System;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

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

            new UpmPackOperation(PackageInfoHelper.GetPackageInfo(packageVersion.name), destination,
                tarballPath => Debug.Log("Package saved successfully at: " + tarballPath),
                error => Debug.LogError("Error: " + error));
        }

        internal static void Publish(PackageInfo packageInfo, string destination, Action<string> onSuccess,
            Action<string> onError)
        {
            new UpmPackOperation(packageInfo, destination, onSuccess, onError);
        }

        static LocalPublishExtension()
        {
            PackageManagerDevelopExtensions.RegisterPublishExtension(new LocalPublishExtension());
        }
    }
}