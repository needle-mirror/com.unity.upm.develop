using System;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.UI;
using UnityEngine.UIElements;

namespace Unity.PackageManagerUI.Develop.Editor
{
#if UNITY_2020_2_OR_NEWER
    [Serializable]
    internal class PackageObject : ScriptableSingleton<PackageObject>
    {
        public string packageName;
    }
#endif
    internal class MenuExtensions : IPackageManagerMenuExtensions
    {
        public static event Action<bool> onShowDevToolsSet = delegate {};

        private const string k_PackagesFolder = "Packages/";
        private const string k_AlwaysShowDevTools = "PackageManager.AlwaysShowDevTools";

        public static bool alwaysShowDevTools
        {
            get { return EditorPrefs.GetBool(k_AlwaysShowDevTools, false); }
            set
            {
                EditorPrefs.SetBool(k_AlwaysShowDevTools, value);
                onShowDevToolsSet(value);
            }
        }

        public void OnAdvancedMenuCreate(DropdownMenu menu)
        {
            if (!Unsupported.IsDeveloperMode())
                return;

            menu.AppendSeparator();
            menu.AppendAction("Internal/Always show development tools", a =>
            {
                OnDevelopmentToolToggle();
            }, a => !Unsupported.IsDeveloperMode() ? DropdownMenuAction.Status.Hidden :
                alwaysShowDevTools? DropdownMenuAction.Status.Checked: DropdownMenuAction.Status.Normal);
        }

        public void OnAddMenuCreate(DropdownMenu menu)
        {
            menu.AppendSeparator("");

            menu.AppendAction("Create Package...", a =>
            {
                var defaultName = PackageCreator.GenerateUniquePackageDisplayName("New Package");
                var createPackage = new PackagesAction("Create", defaultName);
                createPackage.actionClicked += displayName =>
                {
                    createPackage.Hide();
                    PackageManagerWindowAnalytics.SendEvent("createPackage");
#if UNITY_2020_2_OR_NEWER
                    var packageName = PackageCreator.CreatePackage(k_PackagesFolder + displayName).Replace(k_PackagesFolder, "");
                    PackageObject.instance.packageName = packageName;
                    Client.Resolve();
#else
                    PackageCreator.CreatePackage(k_PackagesFolder + displayName);
                    AssetDatabase.Refresh();
                    EditorApplication.delayCall += () => Window.Open(displayName);
#endif
                };

                var parent = EditorWindow.GetWindow<PackageManagerWindow>()
                    .rootVisualElement.Q<PackageManagerToolbar>("topMenuToolbar")
                    .parent;
                parent.Add(createPackage);
                createPackage.Show();
            }, a => DropdownMenuAction.Status.Normal);
        }

        public void OnFilterMenuCreate(DropdownMenu menu) {}

        private static void OnDevelopmentToolToggle()
        {
            alwaysShowDevTools = !alwaysShowDevTools;
        }

#if UNITY_2020_2_OR_NEWER
        [InitializeOnLoadMethod]
        static void SubscribeToEvent()
        {
            Events.registeredPackages += RegisteredPackagesEventHandler;
        }

        static void RegisteredPackagesEventHandler(PackageRegistrationEventArgs packageRegistrationEventArgs)
        {
            if (!string.IsNullOrEmpty(PackageObject.instance.packageName) && packageRegistrationEventArgs.added.Any(package => package.name == PackageObject.instance.packageName))
            {
                Window.Open(PackageObject.instance.packageName);
                PackageObject.instance.packageName = string.Empty;
            }
        }
#endif
    }
}
