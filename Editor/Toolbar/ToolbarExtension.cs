using System;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEditor.PackageManager.ValidationSuite;
using UnityEditor.UIElements.Debugger;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.PackageManagerUI.Develop.Editor {
    class DevelopTools : VisualElement
    {
        Button DevelopButton { get; set; }
        IPackageVersion PackageVersion { get; set; }

        public DevelopTools()
        {
            DevelopButton = new Button();
            DevelopButton.text = "Develop";
            DevelopButton.visible = false;
            DevelopButton.clickable.clicked += DevelopClick;
            
            Add(DevelopButton);
        }
        
        void DevelopClick()
        {
            var details = panel.GetRootVisualElement().Q<PackageDetails>("packageDetails");
            
            details.detailError.ClearError();
            PackageDatabase.instance.Embed(PackageVersion);
            details.RefreshPackageActionButtons();
            
            PackageManagerWindowAnalytics.SendEvent("embed", PackageVersion?.uniqueId);
        }

        private void RefreshDevelopButton()
        {
            var visibleFlag = PackageVersion?.HasTag(PackageTag.Embeddable) ?? false;
            if (visibleFlag)
            {
                var enableButton = !EditorApplication.isCompiling && !PackageDatabase.instance.isInstallOrUninstallInProgress;
                DevelopButton.SetEnabled(enableButton);
            }
            UIUtils.SetElementDisplay(DevelopButton, visibleFlag);
        }

        public void SetPackage(IPackageVersion packageVersion)
        {
            PackageVersion = packageVersion;
            
            RefreshDevelopButton();
        }
    }
    class ToolbarExtension : IPackageManagerToolbarExtension
    {
        public const string PackagePath = "Packages/com.unity.upm.develop/";
        public const string TemplatesPath = "Packages/com.unity.upm.develop/Templates~";
        public const string ResourcesPath = PackagePath + "Editor/Resources/";
        public const string StylePath = ResourcesPath + "Styles/";
        public const string CommonPath = StylePath + "Common.uss";
        public const string LightPath = StylePath + "Light.uss";
        public const string DarkPath = StylePath + "Dark.uss";

        public const string LeftItemsName = "leftItems";
        public const string RightItemsName = "rightItems";

        /* Because when we load resources they could still be loading and Load could return null, 
           we need to retry a few times. This is the maximum times we let the resources try to load properly.
           More than that we expect another problem and we don't want to eat process time over k_MaxDelayTryResources tries.
        */
        private const int k_MaxDelayTryResources = 10;

        internal PackageTestRunner PackageTestRunner { get; set; }
        internal PrepareTools PrepareTools { get; set; }
        internal PublishTools PublishTools { get; set; }
        internal DevelopTools DevelopTools { get; set; }
        IPackageVersion PackageVersion { get; set; }
        VisualElement Toolbar { get; set; }

        public ToolbarExtension() {}
        
        private static void SetStyleSheet(VisualElement element, string path, int leftTry)
        {
            var styleSheet = EditorGUIUtility.Load(path) as StyleSheet;
            if (styleSheet != null)
                element.styleSheets.Add(styleSheet);
            else
            {
                if (leftTry <= 0)
                {
                    Debug.Log("Couldn't load resource " + path);
                    return;
                }
                // we need to do this because the resources might not yet be loaded
                EditorApplication.delayCall += () =>
                {
                    SetStyleSheet(element, path, --leftTry);
                };
            }
        }

        public static void SetStyleSheets(VisualElement element)
        {
            SetStyleSheet(element, EditorGUIUtility.isProSkin ? DarkPath : LightPath, k_MaxDelayTryResources);
            SetStyleSheet(element, CommonPath, k_MaxDelayTryResources);
        }
        
        public void OnPackageSelectionChange(IPackageVersion packageVersion, VisualElement toolbar)
        {
            if (packageVersion == null || toolbar == null)
                return;
            
            PackageVersion = packageVersion;
            Toolbar = toolbar;

            if (PrepareTools == null)
            {
                PrepareTools = new PrepareTools(packageVersion, PackageTestRunner);
                PrepareTools.OnValidate += OnValidate;
            }
            if (PublishTools == null)  PublishTools = new PublishTools();
            if (DevelopTools == null) DevelopTools = new DevelopTools();

            // Put the publish button right before the edit package manifest button, or at the end if it does not exists
            var rightItems = toolbar.Q<VisualElement>(RightItemsName);
            var editPackageManifest = rightItems.Q<VisualElement>("editPackageManifest");
            var publishPosition = editPackageManifest != null ? rightItems.IndexOf(editPackageManifest) : rightItems.childCount;
            
            AddIfNotExists(toolbar.Q<VisualElement>(LeftItemsName), PrepareTools);
            AddIfNotExists(toolbar.Q<VisualElement>(LeftItemsName), DevelopTools);
            AddIfNotExists(rightItems, PublishTools, publishPosition);

            DevelopTools.SetPackage(packageVersion);
            PrepareTools.SetPackage(packageVersion);
            PublishTools.SetPackage(packageVersion);

            var publishTarget = PackageManagerState.Instance.ForPackage(PackageVersion)?.PublishTarget;
            var extension = PackageManagerDevelopExtensions.GetPublishExtension(publishTarget);
        }
        
        void OnValidate()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Debug.LogWarning("Validation suite requires network access and cannot be used offline.");
                return;
            }

            ValidationSuite.ValidatePackage(PackageVersion.versionId(), ValidationType.LocalDevelopment);
            PrepareTools.ShowValidationReport();
        }

        static void AddIfNotExists(VisualElement container, VisualElement child, int position = 0)
        {
            if (!container.Contains(child))
                container.Insert(position, child);
        }

        public void OnWindowDestroy()
        {
            var packageTestRunner = PackageTestRunner ?? PackageTestRunnerSingleton.instance.PackageTestRunner;
            packageTestRunner.UnRegisterCallbacks();
        }
    }
}