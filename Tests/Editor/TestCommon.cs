using System;
using NUnit.Framework;
using UnityEngine;

namespace Unity.PackageManagerUI.Develop.Editor.Tests {
    class TestCommon
    {
        internal ToolbarExtension Extension;
        internal MockTestRunnerApi Mock;
        internal MockPackageVersion mockPackageVersion;
        internal PackageTestRunner PackageTestRunner;
        internal PackageManagerState PackageManagerState;

        PackageManagerState OriginalStateInstance;
		
        // We need to make sure we always create the toolbar with a separate PackageTestRunner and not use the global live singleton
        internal PrepareTools CreateToolbar(MockPackageVersion aMockPackageVersion = null)
        {
            if (aMockPackageVersion!=null)
                return new PrepareTools(aMockPackageVersion, PackageTestRunner);
            else return new PrepareTools(mockPackageVersion, PackageTestRunner);
        }

        [SetUp]
        public void Setup()
        {
            OriginalStateInstance = PackageManagerState.PackageManagerStateInstance;
            PackageManagerState = ScriptableObject.CreateInstance<PackageManagerState>();
            PackageManagerState.PackageManagerStateInstance = PackageManagerState;
                
            Mock = new MockTestRunnerApi();
            PackageTestRunner = new PackageTestRunner();
            PackageTestRunner.TestCompleteMessage = "Mock test completed.";
            PackageTestRunner._Api = Mock;
            Mock.packageTestRunner = PackageTestRunner;
            Extension = new ToolbarExtension();
            Extension.PackageTestRunner = PackageTestRunner;
            mockPackageVersion = new MockPackageVersion("com.unity.upm.develop");
        }

        [TearDown]
        public void TearDown()
        {
            PackageManagerState.PackageManagerStateInstance = OriginalStateInstance;
            OriginalStateInstance = null;
        }
    }
}