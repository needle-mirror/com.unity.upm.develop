using UnityEngine;
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEditor.PackageManager.ValidationSuite;
using Resources = UnityEngine.Resources;

namespace Unity.PackageManagerUI.Develop.Editor {
    class ValidationSuiteReportWindow : EditorWindow
    {
        ValidationSuiteReport Report { get; set; }

        [SerializeField]
        IPackageVersion PackageVersion;
        [SerializeField]
        ValidationTestReport SelectedTest;
        [SerializeField]
        ValidationSuiteReportData ReportData;

        public void OnEnable()
        {
            Report = new ValidationSuiteReport();            
            rootVisualElement.Add(Report);

            Report.OnSelected += OnSelected;

            if (PackageVersion != null)
                SetPackageVersion(PackageVersion, ReportData);
            
            if (SelectedTest != null)
                Report.SelectRow(SelectedTest);
        }

        void OnSelected(ValidationTestReport selected)
        {
            SelectedTest = selected;
        }

        // Work-around to stop dragging when outside the window. Until a proper drag exists
        void OnGUI()
        {
            if (Report.Dragging && this != mouseOverWindow)
                Report.Dragging = false;
        }

        void SetPackageVersion(IPackageVersion packageVersion, ValidationSuiteReportData reportData = null)
        {
            PackageVersion = packageVersion;
            ReportData = reportData ?? ValidationSuite.GetReport(PackageVersion.versionId());
            
            Report?.Init(PackageVersion, ReportData);
        }

        public static void Open(IPackageVersion packageVersion)
        {
            if (IsOpenedWith(packageVersion))
                return;
            if (!ValidationSuite.JsonReportExists(packageVersion.versionId()))
                return;
            
            var dialog = GetWindow<ValidationSuiteReportWindow>(false, "Validation", true);
            dialog.SetPackageVersion(packageVersion);
            dialog.minSize = new Vector2(750, 350);
            dialog.Show();
        }

        public static void UpdateIfOpened(IPackageVersion packageVersion)
        {
            if (IsOpen() && packageVersion != null && ValidationSuite.JsonReportExists(packageVersion.versionId()))
                Open(packageVersion);
        }

        public static bool IsOpen()
        {
            return Resources.FindObjectsOfTypeAll<ValidationSuiteReportWindow>().Any();
        }

        public static bool IsOpenedWith(IPackageVersion packageVersion)
        {
            return Resources.FindObjectsOfTypeAll<ValidationSuiteReportWindow>().Any(window => window.PackageVersion == packageVersion);
        }
    }
}
