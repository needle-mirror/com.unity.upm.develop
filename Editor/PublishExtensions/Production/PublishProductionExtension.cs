using System;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.PackageManager.ValidationSuite;

namespace Unity.PackageManagerUI.Develop.Editor {
    /// <summary>
    /// This class sets up validation for internal publishing. It does not allow publishing by itself.
    /// </summary>
    [InitializeOnLoad]
    internal class PublishProductionExtension : IPublishExtension
    {
        public string Name => "Validate For Production - (Internal)";

        public void OnPublish(IPackageVersion packageVersion)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Debug.LogWarning("Validation suite requires network access and cannot be used offline.");
                return;
            }

            ValidationSuite.ValidatePackage(packageVersion.versionId(), ValidationType.Publishing);
            ValidationSuiteReportWindow.Open(packageVersion);
        }
        
        static PublishProductionExtension()
        {
            if (!Unsupported.IsDeveloperMode())
                return;

            PackageManagerDevelopExtensions.RegisterPublishExtension(new PublishProductionExtension());
        }
    }
}