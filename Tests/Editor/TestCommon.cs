using NUnit.Framework;
using UnityEngine;

namespace Unity.PackageManagerUI.Develop.Editor.Tests
{
    internal class TestCommon
    {
        protected ToolbarExtension m_Extension;
        protected MockTestRunnerApi m_Mock;
        protected PackageTestRunner m_PackageTestRunner;
        protected PackageManagerState m_PackageManagerState;
        protected string m_MockPackageName;
        
        private PackageManagerState m_OriginalStateInstance;

        // We need to make sure we always create the toolbar with a separate PackageTestRunner and not use the global live singleton
        internal PrepareTools CreateToolbar()
        {
            return new PrepareTools(null,null, m_PackageTestRunner);
        }

        [SetUp]
        public void Setup()
        {
            m_OriginalStateInstance = PackageManagerState.instance;
            m_PackageManagerState = ScriptableObject.CreateInstance<PackageManagerState>();
            PackageManagerState.instance = m_PackageManagerState;

            m_Mock = new MockTestRunnerApi();
            m_PackageTestRunner = new PackageTestRunner();
            PackageTestRunner.s_TestCompleteMessage = "Mock test completed.";
            m_PackageTestRunner.api = m_Mock;
            m_Mock.packageTestRunner = m_PackageTestRunner;
            m_Extension = new ToolbarExtension();
            m_Extension.packageTestRunner = m_PackageTestRunner;
            m_MockPackageName = "test";
        }

        [TearDown]
        public void TearDown()
        {
            PackageManagerState.instance = m_OriginalStateInstance;
            m_OriginalStateInstance = null;
        }
    }
}
