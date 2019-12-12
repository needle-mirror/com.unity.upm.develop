using System;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace UnityEditor.PackageManager.UI
{
    /// <summary>
    /// Note that these callbacks will also be called if the Test Runner windows run its tests
    /// </summary>
    [Serializable]
    class PackageTestResultUpdater : ICallbacks
    {
        public event Action<ITestResultAdaptor> OnRunFinished = delegate { };
        
        public void RunStarted(ITestAdaptor testsToRun) {}

        public void RunFinished(ITestResultAdaptor testResults)
        {
            OnRunFinished(testResults);
        }

        public void TestStarted(ITestAdaptor testName) {}
        public void TestFinished(ITestResultAdaptor test) {}
    }
}
