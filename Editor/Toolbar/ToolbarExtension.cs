using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEditor.PackageManager.ValidationSuite;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.PackageManagerUI.Develop.Editor
{
    internal class ToolbarExtension : IPackageManagerToolbarExtension
    {
        private const string k_PackagePath = "Packages/com.unity.upm.develop/";
        public const string k_TemplatesPath = k_PackagePath + "Templates~";
        private const string k_ResourcesPath = k_PackagePath + "Editor/Resources/";
        private const string k_StylePath = k_ResourcesPath + "Styles/";
        private const string k_CommonPath = k_StylePath + "Common.uss";
        private const string k_LightPath = k_StylePath + "Light.uss";
        private const string k_DarkPath = k_StylePath + "Dark.uss";

        internal const string k_LeftItemsName = "leftItems";
        internal const string k_RightItemsName = "rightItems";

        /* Because when we load resources they could still be loading and Load could return null, 
           we need to retry a few times. This is the maximum times we let the resources try to load properly.
           More than that we expect another problem and we don't want to eat process time over k_MaxDelayTryResources tries.
        */
        private const int k_MaxDelayTryResources = 10;

        internal PackageTestRunner packageTestRunner { get; set; }
        internal PrepareTools prepareTools { get; set; }
        internal PublishTools publishTools { get; set; }
        internal DevelopTools pevelopTools { get; set; }

        private IPackageVersion m_PackageVersion { get; set; }
        
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
            SetStyleSheet(element, EditorGUIUtility.isProSkin ? k_DarkPath : k_LightPath, k_MaxDelayTryResources);
            SetStyleSheet(element, k_CommonPath, k_MaxDelayTryResources);
        }

        public void OnPackageSelectionChange(IPackageVersion packageVersion, VisualElement toolbar)
        {
            var package = PackageDatabase.instance.GetPackage(packageVersion);

            if (packageVersion == null || toolbar == null || package == null)
                return;

            m_PackageVersion = packageVersion;

            if (prepareTools == null)
            {
                prepareTools = new PrepareTools(package, packageTestRunner);
                prepareTools.onValidate += OnValidate;
            }
            if (publishTools == null)  publishTools = new PublishTools();
            if (pevelopTools == null) pevelopTools = new DevelopTools();

            // Put the publish button right before the edit package manifest button, or at the end if it does not exists
            var rightItems = toolbar.Q<VisualElement>(k_RightItemsName);
            var editPackageManifest = rightItems.Q<VisualElement>("editPackageManifest");
            var publishPosition = editPackageManifest != null ? rightItems.IndexOf(editPackageManifest) : rightItems.childCount;

            AddIfNotExists(toolbar.Q<VisualElement>(k_LeftItemsName), prepareTools);
            AddIfNotExists(toolbar.Q<VisualElement>(k_LeftItemsName), pevelopTools);
            AddIfNotExists(rightItems, publishTools, publishPosition);

            pevelopTools.SetPackage(package);
            prepareTools.SetPackage(package);
            publishTools.SetPackage(package);

            var publishTarget = PackageManagerState.instance.ForPackage(package.name)?.publishTarget;
            var extension = PackageManagerDevelopExtensions.GetPublishExtension(publishTarget);
        }

        private void OnValidate()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Debug.LogWarning("Validation suite requires network access and cannot be used offline.");
                return;
            }

            ValidationSuite.ValidatePackage(m_PackageVersion.VersionId(), ValidationType.LocalDevelopment);
            prepareTools.ShowValidationReport();
        }

        private static void AddIfNotExists(VisualElement container, VisualElement child, int position = 0)
        {
            if (!container.Contains(child))
                container.Insert(position, child);
        }

        public void OnWindowDestroy()
        {
            var packageTestRunner = this.packageTestRunner ?? PackageTestRunnerSingleton.instance.packageTestRunner;
            packageTestRunner.UnRegisterCallbacks();
        }
    }
}
