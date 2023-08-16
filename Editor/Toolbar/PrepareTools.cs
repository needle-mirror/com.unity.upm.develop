using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite;
using UnityEditor.PackageManager.UI;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Unity.PackageManagerUI.Develop.Editor
{
    internal class PrepareTools : IWindowCreatedHandler, IPackageSelectionChangedHandler, IWindowDestroyHandler
    {
        // ReSharper disable once ClassNeverInstantiated.Local
        private class TryOutReport
        {
            public int exitCode;
            public string message;
        }

        private PackageSelectionArgs m_Selection;

        private IPackageActionButton m_TestRunnerButton;
        private IPackageActionButton m_ValidateButton;
        private IPackageActionButton m_TryoutButton;

        private IPackageActionDropdownItem m_ViewValidateReport;
        private IPackageActionDropdownItem m_ViewTryoutReport;

        private readonly PackageTestRunner m_PackageTestRunner;

        public PrepareTools()
        {
            m_PackageTestRunner = PackageTestRunnerSingleton.instance.packageTestRunner;

            m_PackageTestRunner?.Refresh();
            MenuExtensions.onShowDevToolsSet += OnShowDevToolsSet;
        }

        public void OnWindowCreated(WindowCreatedArgs args)
        {
            var viewReportText = L10n.Tr("View last report...");

            // Test
            m_TestRunnerButton = args.window.AddPackageActionButton();
            m_TestRunnerButton.text = L10n.Tr("Test");
            m_TestRunnerButton.action = TestClicked;

            // Validate
            m_ValidateButton = args.window.AddPackageActionButton();
            m_ValidateButton.text = L10n.Tr("Validate");
            m_ValidateButton.action = ValidateClicked;

            m_ViewValidateReport = m_ValidateButton.AddDropdownItem();
            m_ViewValidateReport.text = viewReportText;
            m_ViewValidateReport.action = ShowValidationReport;

            // Try-out
            m_TryoutButton = args.window.AddPackageActionButton();
            m_TryoutButton.text = L10n.Tr("Try-out");
            m_TryoutButton.action = TryoutClicked;

            m_ViewTryoutReport = m_TryoutButton.AddDropdownItem();
            m_ViewTryoutReport.text = viewReportText;
            m_ViewTryoutReport.action = ShowTryoutReport;
        }

        public void OnPackageSelectionChanged(PackageSelectionArgs args)
        {
            m_Selection = args;
            RefreshAllButtons(args.packageVersion);
        }

        private void RefreshAllButtons(IPackageVersion packageVersion)
        {
            var isInDevelopment = PackageInfoHelper.GetPackageInfo(packageVersion?.name)?.source == UnityEditor.PackageManager.PackageSource.Embedded || 
                                  PackageInfoHelper.GetPackageInfo(packageVersion?.name)?.source == UnityEditor.PackageManager.PackageSource.Local;
            var shouldShow = isInDevelopment || (MenuExtensions.alwaysShowDevTools && packageVersion != null && packageVersion.isInstalled);

            m_TestRunnerButton.visible = shouldShow;
            m_ValidateButton.visible = shouldShow;
            m_TryoutButton.visible = shouldShow;

            RefreshValidationStatus(packageVersion);
            RefreshTryoutStatus(packageVersion);

            ValidationSuiteReportWindow.UpdateIfOpened(packageVersion);
        }

        public void OnWindowDestroy(WindowDestroyArgs args)
        {
            var packageTestRunner = m_PackageTestRunner ?? PackageTestRunnerSingleton.instance.packageTestRunner;
            packageTestRunner.UnRegisterCallbacks();
        }

        private void OnShowDevToolsSet(bool value)
        {
            RefreshAllButtons(m_Selection.packageVersion);
        }

        internal void ShowValidationReport(PackageSelectionArgs args)
        {
            ValidationSuiteReportWindow.Open(args.packageVersion);
            RefreshValidationStatus(args.packageVersion);
        }

        private void ShowTryoutReport(PackageSelectionArgs args)
        {
            string reportFilePath;
            string logFilePath;
            GetTryoutReportAndLogPaths(args.packageVersion, out reportFilePath, out logFilePath);
            if (!string.IsNullOrEmpty(reportFilePath) && File.Exists(reportFilePath))
            {
                var report = JsonUtility.FromJson<TryOutReport>(File.ReadAllText(reportFilePath));
                if (File.Exists(logFilePath))
                    EditorUtility.OpenWithDefaultApp(logFilePath);
                else
                    EditorUtility.DisplayDialog($"Try out package {args.packageVersion.VersionId()}", report.message, "Close");
            }
        }

        internal void RefreshValidationStatus(IPackageVersion packageVersion)
        {
            if (packageVersion == null)
                return;

            m_ViewValidateReport.visible = ValidationSuite.JsonReportExists(packageVersion.VersionId());
        }

        private void RefreshTryoutStatus(IPackageVersion packageVersion)
        {
            string reportFilePath;
            string logFilePath;
            GetTryoutReportAndLogPaths(packageVersion, out reportFilePath, out logFilePath);
            m_ViewTryoutReport.visible = !string.IsNullOrEmpty(reportFilePath) && File.Exists(reportFilePath);
        }

        internal void TestClicked(PackageSelectionArgs args)
        {
            m_PackageTestRunner.Test(args.package?.name, TestMode.PlayMode | TestMode.EditMode);
        }

        private void ValidateClicked(PackageSelectionArgs args)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Debug.LogWarning("Validation suite requires network access and cannot be used offline.");
                return;
            }

            ValidationSuite.ValidatePackage(args.packageVersion.VersionId(), ValidationType.AssetStore);
            ShowValidationReport(args);
        }

        private void TryoutClicked(PackageSelectionArgs args)
        {
            var destination = Path.Combine(Directory.GetCurrentDirectory(), "Temp", "TryOut");
            if (!Directory.Exists(destination))
                Directory.CreateDirectory(destination);

            var packageVersion = args.packageVersion;
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
            LocalPublishExtension.Publish(PackageInfoHelper.GetPackageInfo(args.package.name), destination, path =>
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
                    RefreshTryoutStatus(packageVersion);
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

                    RefreshTryoutStatus(packageVersion);
                };
            }, error =>
                {
                    Debug.LogError($"[TryOut] Error: {error}");
                    EditorUtility.ClearProgressBar();

                    report.exitCode = -1;
                    report.message = error;

                    File.WriteAllText(reportFile, JsonUtility.ToJson(report));

                    RefreshTryoutStatus(packageVersion);
                });
        }

        private static void GetTryoutReportAndLogPaths(IPackageVersion packageVersion, out string tryoutReportPath, out string tryoutLogPath)
        {
            if (packageVersion == null)
            {
                tryoutReportPath = null;
                tryoutLogPath = null;
                return;
            }

            var packageId = $"{packageVersion.name}-{packageVersion.versionString}";
            var tryoutReportFolder = Path.Combine(Directory.GetCurrentDirectory(), "Temp", "TryOut");
            tryoutReportPath = Path.Combine(tryoutReportFolder, $"{packageId}.tryout.report");
            tryoutLogPath = Path.Combine(tryoutReportFolder, $"{packageId}.tryout.log");
        }
    }
}
