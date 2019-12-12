using System;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.PackageManagerUI.Develop.Editor {
    class PublishTools : VisualElement
    {
        DropdownButton PublishButton = new DropdownButton();

        internal IPackageVersion PackageVersion { get; set; }

        public PublishTools()
        {
            ToolbarExtension.SetStyleSheets(this);
            SetupPublishButton();
        }
        
        void SetupPublishButton()
        {
            PublishButton.clickable.clicked += () => PublishButton.OnDropdownButtonClicked();
            PublishButton.name = "publish";
            PublishButton.text = "Publish";
            SetupPublishTargetButton();
            Add(PublishButton);		 
        }

        void SetupPublishTargetButton()
        {
            var menu = new GenericMenu();

            foreach (var extension in PackageManagerDevelopExtensions.PublishExtensions)
            {
                var label = new GUIContent($"{extension.Name}");
                menu.AddItem(label, false, () => OnPublishClicked(extension));
            }

            PublishButton.DropdownMenu = menu;
        }

        void OnPublishClicked(IPublishExtension extension)
        {
            extension?.OnPublish(PackageVersion);
        }

        public void SetPackage(IPackageVersion packageVersion)
        {
            PackageVersion = packageVersion;
            var isInDevelopment = PackageVersion != null && PackageVersion.HasTag(PackageTag.InDevelopment);
            var shouldShow = isInDevelopment || (MenuExtensions.AlwaysShowDevTools && PackageVersion != null && PackageVersion.isInstalled);

            UIUtils.SetElementDisplay(PublishButton, shouldShow);
        }
    }
}