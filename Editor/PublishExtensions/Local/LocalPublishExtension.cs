using System;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEditor.PackageManager.ValidationSuite;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.PackageManagerUI.Develop.Editor {
    [InitializeOnLoad]
    internal class LocalPublishExtension : IPublishExtension
    {
       
        public string Name => "Publish to disk";

        public void OnPublish(IPackageVersion packageVersion)
        {
            var destination = EditorUtility.SaveFolderPanel("Select package destination folder", "", "");
            if (string.IsNullOrEmpty(destination))
                return;
            
            new UpmPackOperation(packageVersion, destination, 
                tarballPath =>Debug.Log("Package saved successfully at: " + tarballPath), 
                error => Debug.LogError("Error: " + error.message));
                
        }
        
        static LocalPublishExtension()
        {
            PackageManagerDevelopExtensions.RegisterPublishExtension(new LocalPublishExtension());
        }
    }
}