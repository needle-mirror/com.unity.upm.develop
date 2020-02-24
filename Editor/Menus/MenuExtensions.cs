using System;
using System.IO;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.PackageManagerUI.Develop.Editor {
    class MenuExtensions : IPackageManagerMenuExtensions
    {
        public static event Action<bool> OnShowDevToolsSet = delegate { };

        const string kAlwaysShowDevTools = "PackageManager.AlwaysShowDevTools";
        
        public static bool AlwaysShowDevTools
        {
            get { return EditorPrefs.GetBool(kAlwaysShowDevTools, false); }
            set { 
                EditorPrefs.SetBool(kAlwaysShowDevTools, value);
                OnShowDevToolsSet(value);
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
                AlwaysShowDevTools ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
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
                    var packagePath = PackageCreator.CreatePackage("Packages/" + displayName);
                    PackageManagerWindowAnalytics.SendEvent("createPackage");
                    AssetDatabase.Refresh();
#if UNITY_2020_1_OR_NEWER
                    EditorApplication.delayCall += () => Window.Open(displayName);
#else
                    EditorApplication.delayCall += () =>
                    {
                        var path = Path.Combine(packagePath, "package.json");
                        var o = AssetDatabase.LoadMainAssetAtPath(path);
                        if (o != null)
                            Selection.activeObject = o;

                        PackageManagerWindow.SelectPackageAndFilter(displayName, PackageFilterTab.InDevelopment, true);
                    };
#endif
                };

                var parent = EditorWindow.GetWindow<PackageManagerWindow>()
                    .rootVisualElement.Q<PackageManagerToolbar>("topMenuToolbar")
                    .parent;
                parent.Add(createPackage);
                createPackage.Show();
            }, a => DropdownMenuAction.Status.Normal);
        }
        public void OnFilterMenuCreate(DropdownMenu menu) { }

        void OnDevelopmentToolToggle()
        {
            AlwaysShowDevTools = !AlwaysShowDevTools;
        }
    }
}
