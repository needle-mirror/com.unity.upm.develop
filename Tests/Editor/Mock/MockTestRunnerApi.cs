using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Internal;
using UnityEditor.TestTools.TestRunner.Api;

namespace Unity.PackageManagerUI.Develop.Editor.Tests {
    class MockTestRunnerApi : ITestRunnerApi
    {
        internal ICallbacks Callback;
        
        internal MockTest EditModeTests = new MockTest();
        internal MockTest PlayModeTests = new MockTest();

        internal int EditModeTestCount = 0;
        internal int PlayModeTestCount = 0;

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
                    EditModeTestCount = result.PassCount;
                if (executionSettings.filter.testMode == TestMode.PlayMode)
                    PlayModeTestCount = result.PassCount;

                Callback.RunFinished(result);
            });
            return Guid.NewGuid().ToString();
        }

        public void RegisterCallbacks<T>(T testCallbacks, int priority = 0) where T : ICallbacks
        {
            Callback = testCallbacks;
        }

        public void UnregisterCallbacks<T>(T testCallbacks) where T : ICallbacks
        {
            Callback = null;
        }
        
        public void RetrieveTestList(TestMode testMode, Action<ITestAdaptor> callback)
        {
            var testList = new MockTest { Name = "Mock Root" };
            var list = new List<ITestAdaptor>();
            testList.Children = list;

            var assemblyNames = packageTestRunner.GetAllPackageTestAssemblies(packageTestRunner.PackageVersion);

            if (testMode.HasFlag(TestMode.EditMode))
            {
                EditModeTests.Name = assemblyNames.FirstOrDefault() + ".dll";
                list.Add(EditModeTests);
            }

            if (testMode.HasFlag(TestMode.PlayMode))
            {
                PlayModeTests.Name = assemblyNames.FirstOrDefault() + ".dll";
                list.Add(PlayModeTests);
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
