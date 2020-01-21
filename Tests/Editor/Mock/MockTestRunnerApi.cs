using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.TestTools.TestRunner.Api;

namespace Unity.PackageManagerUI.Develop.Editor.Tests
{
    class MockTestRunnerApi : ITestRunnerApi
    {
        internal ICallbacks callback;

        internal MockTest editModeTests = new MockTest();
        internal MockTest playModeTests = new MockTest();

        internal int editModeTestCount = 0;
        internal int playModeTestCount = 0;

        public PackageTestRunner packageTestRunner;

        public string Execute(ExecutionSettings executionSettings)
        {
            RetrieveTestList(executionSettings.filter.testMode, testList =>
            {
                var result = new MockResult
                {
                    Test = testList,
                    TestStatus = TestStatus.Passed,
                    PassCount = testList.Children.Sum(t => t.TestCaseCount)
                };

                if (executionSettings.filter.testMode == TestMode.EditMode)
                    editModeTestCount = result.PassCount;
                if (executionSettings.filter.testMode == TestMode.PlayMode)
                    playModeTestCount = result.PassCount;

                callback.RunFinished(result);
            });
            return Guid.NewGuid().ToString();
        }

        public void RegisterCallbacks<T>(T testCallbacks, int priority = 0) where T : ICallbacks
        {
            callback = testCallbacks;
        }

        public void UnregisterCallbacks<T>(T testCallbacks) where T : ICallbacks
        {
            callback = null;
        }

        public void RetrieveTestList(TestMode testMode, Action<ITestAdaptor> callback)
        {
            var testList = new MockTest { Name = "Mock Root" };
            var list = new List<ITestAdaptor>();
            testList.Children = list;

            var assemblyNames = packageTestRunner.GetAllPackageTestAssemblies();

            if (testMode.HasFlag(TestMode.EditMode))
            {
                editModeTests.Name = assemblyNames.FirstOrDefault() + ".dll";
                list.Add(editModeTests);
            }

            if (testMode.HasFlag(TestMode.PlayMode))
            {
                playModeTests.Name = assemblyNames.FirstOrDefault() + ".dll";
                list.Add(playModeTests);
            }

            callback(testList);
        }
    }

    class TestSets
    {
        static public List<ITestAdaptor> CreateTests()
        {
            return new List<ITestAdaptor>
            {
                MockTest.CreateSimple()
            };
        }
    }
}
