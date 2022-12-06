using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.TestTools.TestRunner;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace Unity.PackageManagerUI.Develop.Editor
{
    [Serializable]
    internal class PackageTestRunner
    {
        internal static string s_TestCompleteMessage = "All tests completed!";

        public event Action<bool, string> onTestResultsUpdate = delegate { };
        public event Action onTestResultsEnded = delegate { };

        [NonSerialized]
        private PackageTestResultUpdater m_PackageTestResultUpdater;
        bool isRunning { get { return m_TestMode != 0 || m_Current != 0; } }

        [SerializeField]
        private TestMode m_TestMode = 0;
        [SerializeField]
        private TestMode m_Current = 0;

        [SerializeField]
        private string m_PackageName;
        public string packageName => m_PackageName;

        [SerializeField]
        private int m_TestCaseCount;
        [SerializeField]
        private int m_TestCasePassedCount;

        [NonSerialized]
        private ITestRunnerApi m_Api;
        public ITestRunnerApi api
        {
            get
            {
                if (m_Api == null)
                    m_Api = ScriptableObject.CreateInstance<TestRunnerApi>();
                return m_Api;
            }
            set
            {
                m_Api = value;
            }
        }

        public void Refresh()
        {
            RegisterCallbacks();
        }

        public void UnRegisterCallbacks()
        {
            // Only unregister to TestRunner callbacks if we were already registered
            if (m_PackageTestResultUpdater != null)
            {
                m_PackageTestResultUpdater.onRunFinished -= OnRunFinished;

                api.UnregisterCallbacks(m_PackageTestResultUpdater);
            }
        }

        internal void RegisterCallbacks()
        {
            UnRegisterCallbacks();

            m_PackageTestResultUpdater = new PackageTestResultUpdater();
            m_PackageTestResultUpdater.onRunFinished += OnRunFinished;

            api.RegisterCallbacks(m_PackageTestResultUpdater);
        }

        void OnRunFinished(ITestResultAdaptor testResults)
        {
            // Only process runs we started
            if (isRunning)
            {
                m_Current = 0;
                m_TestCasePassedCount += testResults != null ? testResults.PassCount : 0;

                // We consider runs with skipped tests as passed (and null results means no test for this test mode, which we consider passed also)
                var passed = // There must be at least one test
                    testResults == null ||
                    (testResults.TestStatus == TestStatus.Passed || testResults.TestStatus == TestStatus.Skipped);

                // There must be at least one test total
                if (m_TestMode == 0 && m_TestCasePassedCount == 0)
                    passed = false;

                if (m_TestMode != 0 && passed)
                {
                    Run();
                }
                else
                {
                    //
                    // Test Run is completed
                    if (passed)
                        Debug.Log("All tests passed. " + s_TestCompleteMessage + Environment.NewLine + "    " + m_TestCaseCount + " tests were run.");
                    else
                    {
                        if (m_TestCasePassedCount == 0)
                        {
                            var msg =
                                $"There are no tests in this package ({packageName}). Your package is required to have tests. " +
                                $"If it does have test and was added by a file reference, then make sure it is also added in the " +
                                $"'testables' list of the project manifest";
                            Debug.LogWarning(msg);
                        }
                        else
                        {
                            Debug.LogWarning("Some tests have failed. Please review the test runner for details."); 
                        }
                    }

                    onTestResultsUpdate(passed, packageName);
                    onTestResultsEnded();
                    Reset();
                }
            }
        }

#pragma warning disable 649
        [Serializable]
        private class AssemblyData
        {
            public string name;
        }
#pragma warning restore 649

        public IEnumerable<string> GetAllPackageTestAssemblies()
        {
            var packagePath = string.Format("packages/{0}", packageName);
            if (!Directory.Exists(Path.GetFullPath(packagePath)))
                return Enumerable.Empty<string>();

            var asmdefPaths = AssetDatabase.FindAssets("t:asmdef", new[] { packagePath });
            return asmdefPaths.Select(asmdefPath =>
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(asmdefPath);
                var asmdef = AssetDatabase.LoadAssetAtPath<UnityEditorInternal.AssemblyDefinitionAsset>(assetPath);
                var asset = JsonUtility.FromJson<AssemblyData>(asmdef.text);
                return asset.name;
            });
        }

        /// <summary>
        /// Retrieve the test list for given mode.
        /// </summary>
        /// <param name="testMode"></param>
        /// <param name="assemblyNames">Can be null.</param>
        /// <param name="callback"></param>
        void RetrieveTestList(TestMode testMode, string[] assemblyNames, Action<ITestAdaptor, ExecutionSettings> callback)
        {
            var filter = new Filter { testMode = testMode, assemblyNames = assemblyNames };
            var executionSettings = new ExecutionSettings { filter = filter, filters = new Filter[0] };

            api.RetrieveTestList(testMode, rootTest =>
            {
                callback(rootTest, executionSettings);
            });
        }

        void RunTest(TestMode testMode, string[] assemblyNames)
        {
            RetrieveTestList(testMode, assemblyNames, (rootTest, executionSettings) =>
            {
                var testCount = rootTest.Children
                    .Where(t => assemblyNames.Contains(t.Name.Replace(".dll", "")))
                    .Sum(t => t.TestCaseCount);

                m_TestCaseCount += testCount;

                if (testCount != 0)
                    api.Execute(executionSettings);
                else
                    OnRunFinished(null);
            });
        }

        void Run()
        {
            if (m_TestMode.HasFlag(TestMode.EditMode))
                RunMode(TestMode.EditMode);
            else if (m_TestMode.HasFlag(TestMode.PlayMode))
                RunMode(TestMode.PlayMode);
        }

        void RunMode(TestMode testMode)
        {
            m_Current = testMode;
            m_TestMode &= ~testMode;
            RunTest(testMode, GetAllPackageTestAssemblies().ToArray());
        }

        public void Test(string packageName, TestMode testMode)
        {
            Reset();
            m_PackageName = packageName;

            m_TestMode = testMode;
            ShowTestRunnerWindow();

            Run();
        }

        void Reset()
        {
            m_TestCaseCount = 0;
            m_TestMode = 0;
            m_TestCasePassedCount = 0;
            m_PackageName = null;
        }

        public void ShowTestRunnerWindow()
        {
            var testRunnerWindow = EditorWindow.GetWindow<TestRunnerWindow>("Test Runner");
            testRunnerWindow.Show();
        }
    }
}