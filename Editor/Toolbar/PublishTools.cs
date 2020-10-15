using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.PackageManagerUI.Develop.Editor
{
    internal class PublishTools : VisualElement
    {
        private readonly DropdownButton m_PublishButton = new DropdownButton();

        internal IPackage package { get; set; }
        internal IPackageVersion packageVersion { get; set; }

        public PublishTools()
        {
            ToolbarExtension.SetStyleSheets(this);
            SetupPublishButton();
        }

        private void SetupPublishButton()
        {
            m_PublishButton.clickable.clicked += () => m_PublishButton.OnDropdownButtonClicked();
            m_PublishButton.name = "publish";
            m_PublishButton.text = "Publish";
            SetupPublishTargetButton();
            Add(m_PublishButton);
        }

        private void SetupPublishTargetButton()
        {
            var menu = new GenericMenu();

            foreach (var extension in PackageManagerDevelopExtensions.publishExtensions)
            {
                var label = new GUIContent($"{extension.name}");
                menu.AddItem(label, false, () => OnPublishClicked(extension));
            }

            m_PublishButton.dropdownMenu = menu;
        }

        private void OnPublishClicked(IPublishExtension extension)
        {
            extension?.OnPublish(packageVersion);
        }

        public void SetPackage(IPackage package, IPackageVersion packageVersion)
        {
            this.package = package;
            this.packageVersion = packageVersion;
            var isInDevelopment = packageVersion?.packageInfo?.source == UnityEditor.PackageManager.PackageSource.Embedded;
            var shouldShow = isInDevelopment || (MenuExtensions.alwaysShowDevTools && packageVersion != null && packageVersion.isInstalled);

            UIUtils.SetElementDisplay(m_PublishButton, shouldShow);
        }
    }
}
