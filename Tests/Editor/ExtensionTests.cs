using NUnit.Framework;
using UnityEngine.UIElements;

namespace Unity.PackageManagerUI.Develop.Editor.Tests
{
    internal class ExtensionTests : TestCommon
    {
        public VisualElement CreateToolbarContainer()
        {
            var leftItems = new VisualElement();
            leftItems.name = ToolbarExtension.k_LeftItemsName;
            var rightItems = new VisualElement();
            rightItems.name = ToolbarExtension.k_RightItemsName;
            var container = new VisualElement();
            container.Add(leftItems);
            container.Add(rightItems);

            return container;
        }

        [Ignore("We will need to refactor the PackageDatabase calls before we can run this test to get the package from the PackageVersion")]
        [Test]
        public void Should_Create_Toolbar()
        {
            var container = CreateToolbarContainer();
            m_Extension.OnPackageSelectionChange(null, container);

            Assert.NotNull(m_Extension.prepareTools, "Extension has a toolbar");
            Assert.NotNull(container.Q<PrepareTools>(), "The extension is added to the container");
        }

        [Test]
        public void Should_Unregister_TestRunner_Listener_On_Window_Destroy()
        {
            m_Extension.packageTestRunner = m_PackageTestRunner;
            m_Extension.packageTestRunner.RegisterCallbacks();
            Assert.NotNull(m_Mock.callback, "Is registered to test runner events");
            m_Extension.OnWindowDestroy();
            Assert.IsNull(m_Mock.callback, "Is unregistered to test runner events");
        }

        [Test]
        public void Should_Reset_PackageState_On_PostProcess()
        {
            m_PackageManagerState.ForPackage("com.unity.test1").SetTest(true);
            m_PackageManagerState.ForPackage("com.unity.test2").SetTest(true);

            Assert.IsTrue(m_PackageManagerState.ForPackage("com.unity.test1").isTestSuccess, "Package state has been set");
            PackageAssetPostprocessor.OnPostprocessAllAssets(new[] {"Packages/com.unity.test1/anyfile"}, new string[0], new string[0], new string[0]);

            Assert.IsFalse(m_PackageManagerState.ForPackage("com.unity.test1").isTestSuccess, "Package 1 state is reset");
            Assert.IsTrue(m_PackageManagerState.ForPackage("com.unity.test2").isTestSuccess, "Package 2 state remains unchanged");
        }
    }
}
