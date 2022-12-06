using UnityEditor;
using UnityEditor.PackageManager.UI;

namespace Unity.PackageManagerUI.Develop.Editor
{
    internal class PublishTools : IWindowCreatedHandler, IPackageSelectionChangedHandler
    {
        private PackageSelectionArgs m_Selection;
        private IPackageActionMenu m_PublishMenu;

        public void OnWindowCreated(WindowCreatedArgs args)
        {
            m_PublishMenu = args.window.AddPackageActionMenu();
            m_PublishMenu.text = L10n.Tr("Publish");
            m_PublishMenu.visible = false;

            foreach (var extension in PackageManagerDevelopExtensions.publishExtensions)
            {
                var dropdownItem = m_PublishMenu.AddDropdownItem();
                dropdownItem.text = extension.name;
                dropdownItem.action = args => extension?.OnPublish(args.packageVersion);
            }

            MenuExtensions.onShowDevToolsSet += OnShowDevToolsSet;
        }

        public void OnPackageSelectionChanged(PackageSelectionArgs args)
        {
            m_Selection = args;
            RefreshVisibility();
        }

        private void RefreshVisibility()
        {
            var packageVersion = m_Selection.packageVersion;
            var isInDevelopment = PackageInfoHelper.GetPackageInfo(packageVersion?.name)?.source == UnityEditor.PackageManager.PackageSource.Embedded || 
                                  PackageInfoHelper.GetPackageInfo(packageVersion?.name)?.source == UnityEditor.PackageManager.PackageSource.Local;
            m_PublishMenu.visible = isInDevelopment || (MenuExtensions.alwaysShowDevTools && packageVersion != null && packageVersion.isInstalled);
        }

        private void OnShowDevToolsSet(bool value)
        {
            RefreshVisibility();
        }
    }
}
