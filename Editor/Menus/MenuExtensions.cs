using System;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.UI;

namespace Unity.PackageManagerUI.Develop.Editor
{
    [Serializable]
    internal class PackageObject : ScriptableSingleton<PackageObject>
    {
        public string packageName;
    }

    internal class MenuExtensions : IWindowCreatedHandler
    {
        public static event Action<bool> onShowDevToolsSet = delegate { };

        private const string k_PackagesFolder = "Packages/";
        private const string k_AlwaysShowDevToolsPref = "PackageManager.AlwaysShowDevTools";

        private IMenuDropdownItem m_ShowDevelopmentToolsDropdown;

        public static bool alwaysShowDevTools
        {
            get
            {
                return EditorPrefs.GetBool(k_AlwaysShowDevToolsPref, false);
            }
            set
            {
                EditorPrefs.SetBool(k_AlwaysShowDevToolsPref, value);
                onShowDevToolsSet(value);
            }
        }

        public void OnWindowCreated(WindowCreatedArgs args)
        {
            var defaultName = PackageCreator.GenerateUniquePackageDisplayName("New Package");
            var createPackageDropdown = args.window.addMenu.AddDropdownItem();
            createPackageDropdown.text = "Create package...";
            createPackageDropdown.insertSeparatorBefore = true;
            createPackageDropdown.action = () =>
            {
                args.window.addMenu.ShowInputDropdown(new InputDropdownArgs
                {
                    title = "Create package",
                    defaultValue = defaultName,
                    submitButtonText = "Create",
                    onInputSubmitted = displayName =>
                    {
                        var packageName = PackageCreator.CreatePackage(k_PackagesFolder + displayName).Replace(k_PackagesFolder, "");
                        PackageObject.instance.packageName = packageName;
                        Client.Resolve();
                    }
                });
            };

            if (Unsupported.IsDeveloperMode())
            {
                m_ShowDevelopmentToolsDropdown = args.window.advancedMenu.AddDropdownItem();
                m_ShowDevelopmentToolsDropdown.insertSeparatorBefore = true;
                m_ShowDevelopmentToolsDropdown.text = "Internal/Always show development tools";
                m_ShowDevelopmentToolsDropdown.action = OnDevelopmentToolToggle;
                m_ShowDevelopmentToolsDropdown.isChecked = alwaysShowDevTools;
            }
        }

        private void OnDevelopmentToolToggle()
        {
            alwaysShowDevTools = !alwaysShowDevTools;
            if (m_ShowDevelopmentToolsDropdown != null)
                m_ShowDevelopmentToolsDropdown.isChecked = alwaysShowDevTools;
        }

        [InitializeOnLoadMethod]
        static void SubscribeToEvent()
        {
            Events.registeredPackages += RegisteredPackagesEventHandler;
        }

        static void SelectManifest()
        {
            var pkgFolder = $"Packages/{PackageObject.instance.packageName}";
            var manifestPath = $"{pkgFolder}/package.json";
            var manifest = AssetDatabase.LoadMainAssetAtPath(manifestPath);
            
            if (manifest != null)
            {
                // Select the package manifest so that it is displayed on the inspector
                Selection.activeObject = manifest;
            }
        }

        static void RegisteredPackagesEventHandler(PackageRegistrationEventArgs packageRegistrationEventArgs)
        {
            if (string.IsNullOrEmpty(PackageObject.instance.packageName) || packageRegistrationEventArgs.added.All(package => package.name != PackageObject.instance.packageName))
                return;
            
            // This code could happen before other things are properly initialized (e.g. PackageManagerUI), so
            // we should avoid having hard dependencies if possible (or delay calls)
            SelectManifest();
                
            PackageObject.instance.packageName = string.Empty;
        }
    }
}