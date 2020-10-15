using System;
using UnityEditor.TestTools.TestRunner.Api;

namespace Unity.PackageManagerUI.Develop.Editor
{
    /// <summary>
    /// Note that these callbacks will also be called if the Test Runner windows run its tests
    /// </summary>
    [Serializable]
    internal class PackageTestResultUpdater : ICallbacks
    {
        public event Action<ITestResultAdaptor> onRunFinished = delegate {};

        public void RunStarted(ITestAdaptor testsToRun) {}

        public void RunFinished(ITestResultAdaptor testResults)
        {
            onRunFinished(testResults);
        }

        public void TestStarted(ITestAdaptor testName) {}
        public void TestFinished(ITestResultAdaptor test) {}
    }
}
