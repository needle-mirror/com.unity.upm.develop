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
            var isInDevelopment = packageVersion?.packageInfo?.source == UnityEditor.PackageManager.PackageSource.Embedded;
            m_PublishMenu.visible = isInDevelopment || (MenuExtensions.alwaysShowDevTools && packageVersion != null && packageVersion.isInstalled);
        }

        private void OnShowDevToolsSet(bool value)
        {
            RefreshVisibility();
        }
    }
}
