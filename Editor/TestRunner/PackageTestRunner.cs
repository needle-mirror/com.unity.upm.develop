using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEditor.Scripting.ScriptCompilation;
using UnityEditor.TestTools.TestRunner;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace Unity.PackageManagerUI.Develop.Editor
{
    [Serializable]
    class PackageTestRunner
    {
        internal static string NoTestMessage = "There are no tests in this package. Your package is required to have tests.";
        internal string TestCompleteMessage = "All tests completed!";

        public event Action<bool, IPackageVersion> OnTestResultsUpdate = delegate { };
        public event Action OnTestResultsEnded = delegate { };
        
        [NonSerialized]
        PackageTestResultUpdater packageTestResultUpdater;
        bool Running { get { return TestMode != 0 || Current != 0; } }

        [SerializeField]
        TestMode TestMode = 0;
        [SerializeField]
        TestMode Current = 0;
        
        // Necessary to keep concrete objects in order to serialize and survive domain reload 
        [SerializeField]
        MockPackageVersion mockPackageVersion;
        [SerializeField]
        UpmPackageVersion upmPackageVersion;

        public IPackageVersion PackageVersion
        {
            get
            {
                if (mockPackageVersion != null)
                    return mockPackageVersion;

                return upmPackageVersion;
            }
        }

        [SerializeField]
        internal int TestCaseCount;
        [SerializeField]
        internal int TestCasePassedCount;
        
        [NonSerialized]
        internal ITestRunnerApi _Api;
        ITestRunnerApi Api
        {
            get
            {
                if (_Api == null)
                    _Api = ScriptableObject.CreateInstance<TestRunnerApi>();

                return _Api;
            }
        }

        public void Refresh()
        {
            RegisterCallbacks();
        }
        
        public void UnRegisterCallbacks()
        {
            // Only de-register to TestRunner callbacks if we were already registered
            if (packageTestResultUpdater != null)
            {
                packageTestResultUpdater.OnRunFinished -= OnRunFinished;

                Api.UnregisterCallbacks(packageTestResultUpdater);                
            }            
        }

        internal void RegisterCallbacks()
        {
            UnRegisterCallbacks();

            packageTestResultUpdater = new PackageTestResultUpdater();
            packageTestResultUpdater.OnRunFinished += OnRunFinished;

            Api.RegisterCallbacks(packageTestResultUpdater);
        }

        void OnRunFinished(ITestResultAdaptor testResults)
        {
            // Only process runs we started
            if (Running)
            {
                Current = 0;
                TestCasePassedCount += testResults != null ? testResults.PassCount : 0;

                // We consider runs with skipped tests as passed (and null results means no test for this test mode, which we consider passed also) 
				var passed = // There must be at least one test
                             testResults == null || 
                             (testResults.TestStatus == TestStatus.Passed || testResults.TestStatus == TestStatus.Skipped);
				
				// There must be at least one test total
                if (TestMode == 0 && TestCasePassedCount == 0)
                    passed = false;

                if (TestMode != 0 && passed)
                {
                    Run();
                }
                else
                {
                    //
                    // Test Run is completed                    
                    if (passed)
                        Debug.Log(TestCompleteMessage + Environment.NewLine + "    " + TestCaseCount +" tests were run.");
                    else
                    {
                        if (TestCasePassedCount == 0) 
                            Debug.LogWarning(NoTestMessage);
                        else
                            Debug.LogWarning("Some tests have failed. Please review the test runner for details.");
                    }

                    OnTestResultsUpdate(passed, PackageVersion);
                    OnTestResultsEnded();
                    Reset();                    
                }
            }
        }

        IEnumerable<CustomScriptAssemblyData> GetPackageAssemblyNames(string packageName)
        {
            var packagePath = string.Format("packages/{0}", packageName);
            if (!Directory.Exists(Path.GetFullPath(packagePath)))
                return new List<CustomScriptAssemblyData>();
            
            var asmdefPaths = AssetDatabase.FindAssets("t:asmdef", new [] {packagePath});
            return asmdefPaths.Select(asmdefPath =>
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(asmdefPath);
                var asmdef = AssetDatabase.LoadAssetAtPath<UnityEditorInternal.AssemblyDefinitionAsset>(assetPath);
                var asset = JsonUtility.FromJson<CustomScriptAssemblyData>(asmdef.text);
                return asset;
            });
        }

        /// <summary>
        /// Get all package assemblies 
        /// </summary>
        public IEnumerable<string> GetAllPackageTestAssemblies(IPackageVersion packageVersion)
        {
            return GetPackageAssemblyNames(packageVersion.name)
                .Select(a => a.name);
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

            Api.RetrieveTestList(testMode, rootTest =>
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

                TestCaseCount += testCount;
                
                if (testCount != 0)
                    Api.Execute(executionSettings);
                else
                    OnRunFinished(null);
            });
        }

        void Run()
        {
            if (TestMode.HasFlag(TestMode.EditMode))
                RunMode(TestMode.EditMode);
            else if (TestMode.HasFlag(TestMode.PlayMode))
                RunMode(TestMode.PlayMode);
        }
        
        void RunMode(TestMode testMode)
        {
            Current = testMode;
            TestMode &= ~testMode;
            RunTest(testMode, GetAllPackageTestAssemblies(PackageVersion).ToArray());
        }

        public void Test(IPackageVersion packageVersion, TestMode testMode)
        {
            Reset();

            if (packageVersion is MockPackageVersion)
                mockPackageVersion = packageVersion as MockPackageVersion;
            else
                upmPackageVersion = packageVersion as UpmPackageVersion;
            
            TestMode = testMode;
            ShowTestRunnerWindow();

            Run();
        }

        void Reset()
        {
            TestCaseCount = 0;
            TestMode = 0;
            TestCasePassedCount = 0;
            mockPackageVersion = null;
            upmPackageVersion = null;
        }

        public void ShowTestRunnerWindow()
        {
            var testRunnerWindow = EditorWindow.GetWindow<TestRunnerWindow>("Test Runner");
            testRunnerWindow.Show();            
        }
    }
}
