using System;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEditor.PackageManager.ValidationSuite;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.PackageManagerUI.Develop.Editor 
{
    class PrepareTools : VisualElement
    {
        internal DropdownButton TestRunnerButton = new DropdownButton();
		internal DropdownButton ValidateButton = new DropdownButton();

        PackageTestRunner PackageTestRunner;

        public event Action OnValidate = delegate { };

        internal IPackageVersion PackageVersion { get; set; }
        
        public PrepareTools(IPackageVersion packageVersion, PackageTestRunner packageTestRunner = null)
        {
            ToolbarExtension.SetStyleSheets(this);

            PackageTestRunner = packageTestRunner ?? PackageTestRunnerSingleton.instance.PackageTestRunner;

            PackageVersion = packageVersion;

            RefreshDevelopmentButtons();

            SetupTestButton();
            SetupValidateButton();
            
            PackageTestRunner.Refresh();
            SetPackageTestRunner();
            
            MenuExtensions.OnShowDevToolsSet += OnShowDevToolsSet;
        }
        
        void OnShowDevToolsSet(bool value)
        {
            RefreshActionsDisplay();
        }
        
        public void SetPackage(IPackageVersion packageVersion)
        {
            PackageVersion = packageVersion;
            RefreshDevelopmentButtons();
            RefreshValidationStatus();
            ValidationSuiteReportWindow.UpdateIfOpened(packageVersion);
        }

        void SetPackageTestRunner()
        {
            if (PackageTestRunner != null)
                PackageTestRunner.OnTestResultsUpdate -= UpdateTestResults;

            PackageTestRunner.OnTestResultsUpdate += UpdateTestResults;
        }

        void UpdateTestResults(bool passed, IPackageVersion packageVersion)
        {
            PackageManagerState.Instance.ForPackage(packageVersion).SetTest(passed);
            RefreshDevelopmentButtons();
        }

        void SetupTestButton()
        {
            // Always enabled not matter what the publishing target is.
            TestRunnerButton.text = "Test";
            TestRunnerButton.clickable.clicked += TestClicked;

            Add(TestRunnerButton);            
        }

        void SetupValidateButton()
        {
            ValidateButton.text = "Validate";
            ValidateButton.clickable.clicked += ValidateClicked;
            RefreshValidationStatus();

            Add(ValidateButton);            
        }
        
        internal void ShowValidationReport()
        {
            ValidationSuiteReportWindow.Open(PackageVersion);
            RefreshValidationStatus();
        }
        
        internal void RefreshDevelopmentButtons(DevelopmentState developmentState = null)
        {
            if (PackageVersion == null)
                return;

            var isInDevelopment = PackageVersion?.HasTag(PackageTag.InDevelopment) ?? false;
            RefreshActionsDisplay();

            if (developmentState == null && isInDevelopment)
                developmentState = PackageManagerState.Instance.ForPackage(PackageVersion.name);

            if (developmentState != null && developmentState.PackageName == PackageVersion.name)
            {
                developmentState.OnDevelopmentStateUpdate -= RefreshDevelopmentButtons;
                developmentState.OnDevelopmentStateUpdate += RefreshDevelopmentButtons;
            }
            
            RefreshActionsStatus(developmentState);
        }

        void RefreshActionsDisplay()
        {
            if (PackageVersion == null)
                return;

            var isInDevelopment = PackageVersion?.HasTag(PackageTag.InDevelopment) ?? false;
            var shouldShow = isInDevelopment || (MenuExtensions.AlwaysShowDevTools && PackageVersion.isInstalled);
            UIUtils.SetElementDisplay(TestRunnerButton, shouldShow);
            UIUtils.SetElementDisplay(ValidateButton, shouldShow);
        }

        void RefreshActionsStatus(DevelopmentState developmentState)
        {
            if (developmentState?.Test != DropdownStatus.None)
                TestRunnerButton.DropdownMenu = CreateStandardDropdown(state => PackageTestRunner.ShowTestRunnerWindow());
            else
                TestRunnerButton.DropdownMenu = null;

            if (developmentState != null)
                TestRunnerButton.Status = developmentState.Test;

            RefreshValidationStatus();
        }

        internal void RefreshValidationStatus()
        {
            if (PackageVersion == null)
                return;

            if (!ValidationSuite.JsonReportExists(PackageVersion.versionId()))
            {
                ValidateButton.Status = DropdownStatus.None;
                ValidateButton.DropdownMenu = null;
            }
            else
            {
                var report = ValidationSuite.GetReport(PackageVersion.versionId());
                if (report.TestResult != TestState.Succeeded)
                    ValidateButton.Status = DropdownStatus.Error;
                else
                    ValidateButton.Status = DropdownStatus.Success;
                
                ValidateButton.DropdownMenu = CreateStandardDropdown(state => ShowValidationReport());                
            }
        }

        internal void TestClicked()
        {
            PackageTestRunner.Test(PackageVersion, TestMode.PlayMode | TestMode.EditMode);
        }

        void ValidateClicked()
        {
            OnValidate();
        }

        GenericMenu CreateStandardDropdown(Action<DevelopmentState> viewReportAction)
        {
            var menu = new GenericMenu();
            var viewReportItem = new GUIContent("View report...");
            menu.AddItem(viewReportItem, false, delegate
            {
                var state = PackageManagerState.Instance.ForPackage(PackageVersion.name);
                if (state != null)
                    viewReportAction(state);
            });

            return menu;
        }
    }
}