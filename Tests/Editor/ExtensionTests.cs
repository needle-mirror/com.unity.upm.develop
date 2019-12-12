using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.PackageManagerUI.Develop.Editor.Tests
{
    class ExtensionTests : TestCommon
    {
        public VisualElement CreateToolbarContainer()
        {
            var leftItems = new VisualElement();
            leftItems.name = ToolbarExtension.LeftItemsName;
            var rightItems = new VisualElement();
            rightItems.name = ToolbarExtension.RightItemsName;
            var container = new VisualElement();
            container.Add(leftItems);
            container.Add(rightItems);

            return container;
        }
        
        [Test]
        public void Should_Create_Toolbar()
        {
            var container = CreateToolbarContainer();
            Extension.OnPackageSelectionChange(mockPackageVersion, container);
            
            Assert.NotNull(Extension.PrepareTools, "Extension has a toolbar");
            Assert.NotNull(container.Q<PrepareTools>(), "The extension is added to the container");
        }

        [Test]
        public void Should_Unregister_TestRunner_Listener_On_Window_Destroy()
        {
            Extension.PackageTestRunner = PackageTestRunner;
            Extension.PackageTestRunner.RegisterCallbacks();
            Assert.NotNull(Mock.Callback, "Is registered to test runner events");
            Extension.OnWindowDestroy();
            Assert.IsNull(Mock.Callback, "Is de-registered to test runner events");
        }

        [Test]
        public void Should_Reset_PackageState_On_PostProcess()
        {
            PackageManagerState.ForPackage("com.unity.test1").SetTest(true);
            PackageManagerState.ForPackage("com.unity.test2").SetTest(true);
            
            Assert.IsTrue(PackageManagerState.ForPackage("com.unity.test1").IsTestSuccess, "Package state has been set");
            PackageAssetPostprocessor.OnPostprocessAllAssets(new[] {"Packages/com.unity.test1/anyfile"}, new string[0], new string[0], new string[0]);
            
            Assert.IsFalse(PackageManagerState.ForPackage("com.unity.test1").IsTestSuccess, "Package 1 state is reset");
            Assert.IsTrue(PackageManagerState.ForPackage("com.unity.test2").IsTestSuccess, "Package 2 state remains unchanged");
        }
    }
}
