using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Unity.PackageManagerUI.Develop.Editor.Tests
{
    internal class PrepareToolbarTests : TestCommon
    {
        PrepareTools MockTestRunner(List<MockTest> editModeTests = null, List<MockTest> playModeTests = null)
        {
            if (editModeTests == null) editModeTests = new List<MockTest>();
            if (playModeTests == null) playModeTests = new List<MockTest>();

            // Need to use this package name since its the only one that has assembly names we can be sure exists
            var toolbar = CreateToolbar();
            m_Mock.editModeTests.Children = editModeTests;
            m_Mock.playModeTests.Children = playModeTests;

            return toolbar;
        }

        [Test]
        public void Should_Create_Toolbar()
        {
            Assert.IsNotNull(CreateToolbar());
        }

        static IEnumerable<TestCaseData> EditAndPlayModeTests()
        {
            yield return new TestCaseData(new List<MockTest> {new MockTest()}, new List<MockTest>()).SetName("Editmode 1 - Playmode 0");
            yield return new TestCaseData(new List<MockTest> {new MockTest()},  new List<MockTest> {new MockTest()}).SetName("Editmode 1 - Playmode 1");
            yield return new TestCaseData(new List<MockTest> {new MockTest(), new MockTest()},  new List<MockTest> {new MockTest()}).SetName("Editmode 2 - Playmode 1");
        }

        [Ignore("We will need to refactor the public interfaces to recreate these tests properly")]
        [Test, TestCaseSource(nameof(EditAndPlayModeTests))]
        public void Should_Run_Package_Tests(List<MockTest> editModeTests, List<MockTest> playModeTests)
        {
            m_PackageTestRunner.onTestResultsEnded += () =>
            {
                // Make sure only the expected test was run
                Assert.That(editModeTests, Has.Count.EqualTo(m_Mock.editModeTestCount), "Edit mode tests has been run.");
                Assert.That(playModeTests, Has.Count.EqualTo(m_Mock.playModeTestCount), "Play mode tests have been run.");

                // Make sure its effects are validated
                Assert.IsTrue(PackageManagerState.instance.ForPackage(m_MockPackageName).isTestSuccess, "The test run is a success.");
            };

            MockTestRunner(editModeTests, playModeTests).TestClicked();
        }

        [Test]
        public void Should_Warn_When_No_Tests()
        {
            m_PackageTestRunner.onTestResultsEnded += () => Assert.IsFalse(PackageManagerState.instance.ForPackage(m_MockPackageName).isTestSuccess, "The test run did not succeed.");

            MockTestRunner().TestClicked();

            LogAssert.Expect(LogType.Warning, PackageTestRunner.k_NoTestMessage);
        }
    }
}
