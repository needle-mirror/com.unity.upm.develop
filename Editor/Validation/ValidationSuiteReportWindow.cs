using UnityEngine;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite;
using UnityEditor.PackageManager.UI;
using Resources = UnityEngine.Resources;

namespace Unity.PackageManagerUI.Develop.Editor
{
    internal class ValidationSuiteReportWindow : EditorWindow
    {
        private ValidationSuiteReport m_Report { get; set; }

        [SerializeField]
        private IPackageVersion m_PackageVersion;
        [SerializeField]
        private ValidationTestReport m_SelectedTest;
        [SerializeField]
        private ValidationSuiteReportData m_ReportData;

        public void OnEnable()
        {
            m_Report = new ValidationSuiteReport();
            rootVisualElement.Add(m_Report);

            m_Report.onSelected += OnSelected;

            if (m_PackageVersion != null)
                SetPackageVersion(m_PackageVersion, m_ReportData);

            if (m_SelectedTest != null)
                m_Report.SelectRow(m_SelectedTest);
        }

        void OnSelected(ValidationTestReport selected)
        {
            m_SelectedTest = selected;
        }

        // Work-around to stop dragging when outside the window. Until a proper drag exists
        void OnGUI()
        {
            if (m_Report.isDragging && this != mouseOverWindow)
                m_Report.isDragging = false;
        }

        void SetPackageVersion(IPackageVersion packageVersion, ValidationSuiteReportData reportData = null)
        {
            m_PackageVersion = packageVersion;
            m_ReportData = reportData ?? ValidationSuite.GetReport(m_PackageVersion.VersionId());

            m_Report?.Init(m_PackageVersion, m_ReportData);
        }

        public static void Open(IPackageVersion packageVersion)
        {
            if (IsOpenedWith(packageVersion))
                return;
            if (!ValidationSuite.JsonReportExists(packageVersion.VersionId()))
                return;

            var dialog = GetWindow<ValidationSuiteReportWindow>(false, "Validation", true);
            dialog.SetPackageVersion(packageVersion);
            dialog.minSize = new Vector2(750, 350);
            dialog.Show();
        }

        public static void UpdateIfOpened(IPackageVersion packageVersion)
        {
            if (IsOpen() && packageVersion != null && ValidationSuite.JsonReportExists(packageVersion.VersionId()))
                Open(packageVersion);
        }

        public static bool IsOpen()
        {
            return Resources.FindObjectsOfTypeAll<ValidationSuiteReportWindow>().Any();
        }

        public static bool IsOpenedWith(IPackageVersion packageVersion)
        {
            return Resources.FindObjectsOfTypeAll<ValidationSuiteReportWindow>().Any(window => window.m_PackageVersion == packageVersion);
        }
    }
}
