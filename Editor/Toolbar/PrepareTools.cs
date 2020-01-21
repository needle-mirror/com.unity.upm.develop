using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEditor.PackageManager.ValidationSuite;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;

namespace Unity.PackageManagerUI.Develop.Editor
{
    internal class PrepareTools : VisualElement
    {
        private DropdownButton m_TestRunnerButton = new DropdownButton();
        private DropdownButton m_ValidateButton = new DropdownButton();
        private DropdownButton m_TryoutButton = new DropdownButton();

        // ReSharper disable once ClassNeverInstantiated.Local
        private class TryOutReport
        {
            public int exitCode;
            public string message;
        }

        private readonly PackageTestRunner m_PackageTestRunner;

        public event Action onValidate = delegate {};

        internal IPackage package { get; set; }

        internal IPackageVersion packageVersion { get; set; }

        public PrepareTools(IPackage package, PackageTestRunner packageTestRunner = null)
        {
            ToolbarExtension.SetStyleSheets(this);

            m_PackageTestRunner = packageTestRunner ?? PackageTestRunnerSingleton.instance.packageTestRunner;

            this.package = package;
            packageVersion = this.package?.versions?.primary;

            RefreshDevelopmentButtons();

            SetupTestButton();
            SetupValidateButton();
            SetupTryoutButton();

            m_PackageTestRunner.Refresh();
            SetPackageTestRunner();

            MenuExtensions.onShowDevToolsSet += OnShowDevToolsSet;
        }

        private void OnShowDevToolsSet(bool value)
        {
            RefreshActionsDisplay();
        }

        public void SetPackage(IPackage package)
        {
            this.package = package;
            packageVersion = this.package?.versions?.primary;
            RefreshDevelopmentButtons();
            RefreshValidationStatus();
            RefreshTryoutStatus();
            ValidationSuiteReportWindow.UpdateIfOpened(packageVersion);
        }

        private void SetPackageTestRunner()
        {
            if (m_PackageTestRunner != null)
                m_PackageTestRunner.onTestResultsUpdate -= UpdateTestResults;

            m_PackageTestRunner.onTestResultsUpdate += UpdateTestResults;
        }

        private void UpdateTestResults(bool passed, string packageName)
        {
            PackageManagerState.instance.ForPackage(packageName)?.SetTest(passed);
            RefreshDevelopmentButtons();
        }

        private void SetupTestButton()
        {
            // Always enabled not matter what the publishing target is.
            m_TestRunnerButton.text = "Test";
            m_TestRunnerButton.clickable.clicked += TestClicked;

            Add(m_TestRunnerButton);
        }

        private void SetupValidateButton()
        {
            m_ValidateButton.text = "Validate";
            m_ValidateButton.clickable.clicked += ValidateClicked;
            RefreshValidationStatus();

            Add(m_ValidateButton);
        }

        private void SetupTryoutButton()
        {
            m_TryoutButton.text = "Try-out";
            m_TryoutButton.clickable.clicked += TryoutClicked;
            RefreshTryoutStatus();

            Add(m_TryoutButton);
        }

        internal void ShowValidationReport()
        {
            ValidationSuiteReportWindow.Open(packageVersion);
            RefreshValidationStatus();
        }

        private void RefreshDevelopmentButtons(DevelopmentState developmentState = null)
        {
            if (packageVersion == null)
                return;

            var isInDevelopment = packageVersion?.HasTag(PackageTag.InDevelopment) ?? false;
            RefreshActionsDisplay();

            if (developmentState == null && isInDevelopment)
                developmentState = PackageManagerState.instance.ForPackage(packageVersion.name);

            if (developmentState != null && developmentState.packageName == packageVersion.name)
            {
                developmentState.onDevelopmentStateUpdate -= RefreshDevelopmentButtons;
                developmentState.onDevelopmentStateUpdate += RefreshDevelopmentButtons;
            }

            RefreshActionsStatus(developmentState);
        }

        private void RefreshActionsDisplay()
        {
            if (packageVersion == null)
                return;
            var isInDevelopment = packageVersion?.HasTag(PackageTag.InDevelopment) ?? false;
            var shouldShow = isInDevelopment || (MenuExtensions.alwaysShowDevTools && packageVersion.isInstalled);
            UIUtils.SetElementDisplay(m_TestRunnerButton, shouldShow);
            UIUtils.SetElementDisplay(m_ValidateButton, shouldShow);
            UIUtils.SetElementDisplay(m_TryoutButton, shouldShow);
        }

        private void RefreshActionsStatus(DevelopmentState developmentState)
        {
            if (developmentState?.test != DropdownStatus.None)
                m_TestRunnerButton.DropdownMenu = CreateStandardDropdown(state => m_PackageTestRunner.ShowTestRunnerWindow());
            else
                m_TestRunnerButton.DropdownMenu = null;

            if (developmentState != null)
                m_TestRunnerButton.Status = developmentState.test;

            RefreshValidationStatus();
            RefreshTryoutStatus();
        }

        internal void RefreshValidationStatus()
        {
            if (packageVersion == null)
                return;

            if (!ValidationSuite.JsonReportExists(packageVersion.VersionId()))
            {
                m_ValidateButton.Status = DropdownStatus.None;
                m_ValidateButton.DropdownMenu = null;
            }
            else
            {
                var report = ValidationSuite.GetReport(packageVersion.VersionId());
                if (report.TestResult != TestState.Succeeded)
                    m_ValidateButton.Status = DropdownStatus.Error;
                else
                    m_ValidateButton.Status = DropdownStatus.Success;

                m_ValidateButton.DropdownMenu = CreateStandardDropdown(state => ShowValidationReport());
            }
        }

        private void RefreshTryoutStatus()
        {
            if (packageVersion == null)
                return;

            var packageId = $"{packageVersion.name}-{packageVersion.versionString}";
            var reportFile = Path.Combine(Directory.GetCurrentDirectory(), "Temp", "TryOut", $"{packageId}.tryout.report");
            if (File.Exists(reportFile))
            {
                var logFile = Path.Combine(Directory.GetCurrentDirectory(), "Temp", "TryOut", $"{packageId}.tryout.log");
                var report = JsonUtility.FromJson<TryOutReport>(File.ReadAllText(reportFile));
                m_TryoutButton.Status = report.exitCode == 0 ? DropdownStatus.Success : DropdownStatus.Error;
                m_TryoutButton.DropdownMenu = CreateStandardDropdown(state =>
                {
                    if (File.Exists(logFile))
                        EditorUtility.OpenWithDefaultApp(logFile);
                    else
                        EditorUtility.DisplayDialog($"Try out package {packageId}", report.message, "Close");
                });
                return;
            }

            m_TryoutButton.Status = DropdownStatus.None;
            m_TryoutButton.DropdownMenu = null;
        }

        internal void TestClicked()
        {
            m_PackageTestRunner.Test(package?.name, TestMode.PlayMode | TestMode.EditMode);
        }

        private void ValidateClicked()
        {
            onValidate();
        }

        private void TryoutClicked()
        {
            var destination = Path.Combine(Directory.GetCurrentDirectory(), "Temp", "TryOut");
            if (!Directory.Exists(destination))
                Directory.CreateDirectory(destination);

            var packageId = $"{packageVersion.name}-{packageVersion.versionString}";
            var tarballName = $"{packageId}.tgz";
            var tarballFile = Path.Combine(destination, tarballName);
            if (File.Exists(tarballFile))
                File.Delete(tarballFile);

            var logFile = Path.Combine(destination, $"{packageId}.tryout.log");
            if (File.Exists(logFile))
                File.Delete(logFile);

            var reportFile = Path.Combine(destination, $"{packageId}.tryout.report");
            if (File.Exists(reportFile))
                File.Delete(reportFile);

            var report = new TryOutReport();
            EditorUtility.DisplayProgressBar($"Try out package {packageId}", $"Publish package {packageId} to tarball...", 1.0f);
            LocalPublishExtension.Publish(packageVersion, destination, path =>
            {
                var p = new Process();
                var appPath = EditorApplication.applicationPath;
#if UNITY_EDITOR_OSX
                appPath += "/Contents/MacOS/Unity";
#endif
                p.StartInfo.FileName = appPath;

                var projectPath = Path.Combine(Path.GetTempPath(), GUID.Generate().ToString());
                if (!Directory.Exists(projectPath))
                    Directory.CreateDirectory(projectPath);

                Directory.CreateDirectory(Path.Combine(projectPath, "Assets"));
                Directory.CreateDirectory(Path.Combine(projectPath, "Packages"));

                var manifest = "{\"dependencies\":{\"" + packageVersion.name + "\":\"file:" + path.Replace('\\', '/') + '/' + tarballName + "\"}}";
                File.WriteAllText(Path.Combine(projectPath, "Packages", "manifest.json"), manifest);

                p.StartInfo.Arguments = "-upmNoDefaultPackages -forgetProjectPath -projectPath \"" + projectPath + "\" -cleanTestPrefs -logFile \"" + logFile + "\"";

                var message = string.Empty;
                try
                {
                    if (!p.Start())
                        message = "Unable to start process " + p.StartInfo.FileName;
                }
                catch (Exception e)
                {
                    message = e.Message;
                }

                if (!string.IsNullOrEmpty(message))
                {
                    Debug.LogError($"[TryOut] Error: {message}");
                    EditorUtility.ClearProgressBar();

                    report.exitCode = -1;
                    report.message = message;

                    File.WriteAllText(reportFile, JsonUtility.ToJson(report));
                    RefreshTryoutStatus();
                    return;
                }

                EditorUtility.DisplayProgressBar($"Try out package {packageId}", $"Waiting process {p.Id} to end...", 1.0f);
                EditorApplication.delayCall += () =>
                {
                    p.WaitForExit();
                    EditorUtility.ClearProgressBar();

                    report.exitCode = p.ExitCode;
                    report.message = string.Empty;

                    File.WriteAllText(reportFile, JsonUtility.ToJson(report));

                    RefreshTryoutStatus();
                };
            }, error =>
                {
                    Debug.LogError($"[TryOut] Error: {error.message}");
                    EditorUtility.ClearProgressBar();

                    report.exitCode = (int)error.errorCode;
                    report.message = error.message;

                    File.WriteAllText(reportFile, JsonUtility.ToJson(report));

                    RefreshTryoutStatus();
                });
        }

        private GenericMenu CreateStandardDropdown(Action<DevelopmentState> viewReportAction)
        {
            var menu = new GenericMenu();
            var viewReportItem = new GUIContent("View last report...");
            menu.AddItem(viewReportItem, false, delegate
            {
                var state = PackageManagerState.instance.ForPackage(packageVersion.name);
                if (state != null)
                    viewReportAction(state);
            });

            return menu;
        }
    }
}
