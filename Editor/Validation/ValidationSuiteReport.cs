using System;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.PackageManagerUI.Develop.Editor
{
    internal class ValidationSuiteReport : VisualElement
    {
        internal const string k_PackagePath = "Packages/com.unity.upm.develop/";
        internal const string k_ResourcesPath = k_PackagePath + "Editor/Resources/";
        private const string k_StylePath = k_ResourcesPath + "Styles/";
        private const string k_CommonPath = k_StylePath + "Common.uss";
        private const string k_LightPath = k_StylePath + "Light.uss";
        private const string k_DarkPath = k_StylePath + "Dark.uss";

        private const string k_SelectedClass = "selected";

        public event Action<ValidationTestReport> onSelected = delegate {};

        internal IPackageVersion packageVersion { get; set; }

        private VisualElement m_Content;
        private ScrollView m_TestList;
        private ReportRow m_Selected;
        private VisualElement m_DetailsContainer;
        private TextField m_Details;
        private Label m_Header;
        private Label m_ValidationType;
        private VisualElement m_Summary;

        public ValidationSuiteReportData report { get; set; }

        public bool isDragging { get; set; }
        private float m_DragStart;
        private float m_DragStartHeight;

        public ValidationSuiteReport()
        {
            SetStyleSheets(this);

            name = "validationReport";
            this.StretchToParentSize();

            style.justifyContent = Justify.SpaceBetween;

            Add(CreateContent());

            RegisterCallback<AttachToPanelEvent>(e => OnEnterPanel());
            RegisterCallback<DetachFromPanelEvent>(e => OnLeavePanel());

            OnEnterPanel();
        }

        private static void SetStyleSheet(VisualElement element, string path)
        {
            var styleSheet = EditorGUIUtility.Load(path) as StyleSheet;
            if (styleSheet != null)
                element.styleSheets.Add(styleSheet);
        }

        internal static void SetStyleSheets(VisualElement element)
        {
            SetStyleSheet(element, EditorGUIUtility.isProSkin ? k_DarkPath : k_LightPath);
            SetStyleSheet(element, k_CommonPath);
        }

        void OnEnterPanel()
        {
            if (panel == null)
                return;
            isDragging = false;
            panel.visualTree.RegisterCallback<KeyDownEvent>(OnKeyDownEvent);
        }

        void OnLeavePanel()
        {
            if (panel == null)
                return;

            isDragging = false;
            panel.visualTree.UnregisterCallback<KeyDownEvent>(OnKeyDownEvent);
        }

        void OnKeyDownEvent(KeyDownEvent evt)
        {
            switch (evt.keyCode)
            {
                case KeyCode.UpArrow:
                {
                    SelectBy(-1);
                    evt.PreventDefault();
                    evt.StopImmediatePropagation();
                    break;
                }
                case KeyCode.DownArrow:
                {
                    SelectBy(1);
                    evt.PreventDefault();
                    evt.StopImmediatePropagation();
                    break;
                }
            }
        }

        void SelectBy(int delta = 1)
        {
            if (!report.Tests.Any())
                return;

            var currentTestIndex = report.Tests.FindIndex(test => test == m_Selected.test);
            var targetIndex = Math.Max(Math.Min(currentTestIndex + delta, report.Tests.Count - 1), 0);
            var targetTest = report.Tests[targetIndex];

            SelectRow(targetTest);
        }

        ReportRow GetForTest(ValidationTestReport targetTest)
        {
            return m_TestList.Query<ReportRow>().ToList().Find(row => row.test.TestName == targetTest.TestName);
        }

        public void Init(IPackageVersion packageVersion, ValidationSuiteReportData reportData)
        {
            if (packageVersion == null)
                return;
            if (packageVersion != this.packageVersion)
                onSelected(null);

            this.packageVersion = packageVersion;

            m_Header.text = $"{this.packageVersion.displayName} Validation Report ({reportData.EndTime})";
            m_ValidationType.text = $"Validation Type: {reportData.Type}";

            report = reportData;
            m_TestList.Clear();
            m_Details.value = "";

            if (report != null)
            {
                m_Header.tooltip = $"Test was run using the {reportData.Type} validation type.";
                if (report.Tests != null)
                    foreach (var test in report.Tests)
                        m_TestList.Add(new ReportRow(test, SelectRow));
            }
        }

        public void SelectRow(ValidationTestReport targetTest)
        {
            SelectRow(GetForTest(targetTest));
        }

        public void SelectRow(ReportRow row)
        {
            if (m_Selected != null)
                m_Selected.RemoveFromClassList(k_SelectedClass);

            m_Selected = row;

            if (m_Selected != null)
            {
                onSelected(m_Selected.test);
                m_Selected.AddToClassList(k_SelectedClass);
                m_Details.value = $"{m_Selected.test.TestDescription}\n\n{string.Join("\n\n", m_Selected.test.TestOutput.Select(x => x.Output).ToArray())}";
            }
            else
                m_Details.value = "";

            UIUtils.ScrollIfNeeded(m_TestList, row);
        }

        VisualElement CreateContent()
        {
            m_Content = new VisualElement();
            m_Content.name = "content";

            m_Summary = new VisualElement();
            m_Summary.name = "summary";

            m_Header = new Label();
            m_Header.name = "header";
            m_Summary.Add(m_Header);

            m_ValidationType = new Label();
            m_ValidationType.name = "validationType";
            m_ValidationType.text = "Validation Type: None";
            m_Summary.Add(m_ValidationType);

            m_Content.Add(m_Summary);

            m_TestList = new ScrollView();
            m_TestList.name = "testList";
            m_Content.Add(m_TestList);

            m_Content.Add(CreateDetails());

            return m_Content;
        }

        VisualElement CreateDetails()
        {
            m_DetailsContainer = new VisualElement();
            m_DetailsContainer.name = "detailsContainer";
            var splitter = new VisualElement();
            splitter.name = "splitter";
            splitter.RegisterCallback<MouseDownEvent>(evt =>
            {
                isDragging = true;
                m_DragStart = splitter.LocalToWorld(evt.localMousePosition).y;
                m_DragStartHeight = m_DetailsContainer.layout.height;
            });
            RegisterCallback<MouseUpEvent>(evt => isDragging = false);
            RegisterCallback<MouseMoveEvent>(OnDrag);
            m_Details = new TextField();
            m_Details.name = "details";
            m_Details.multiline = true;
            m_Details.RegisterCallback<KeyDownEvent>(evt =>
            {
                // Don't allow modification of this textfield
                evt.PreventDefault();
                evt.StopImmediatePropagation();
            });
            var detailsContent = new ScrollView();
            detailsContent.name = "detailsContent";
            detailsContent.Add(m_Details);
            m_DetailsContainer.Add(splitter);
            m_DetailsContainer.Add(detailsContent);

            return m_DetailsContainer;
        }

        void OnDrag(MouseMoveEvent evt)
        {
            if (!isDragging)
                return;

            var delta = m_DragStart - this.LocalToWorld(evt.localMousePosition).y;
            m_DetailsContainer.style.minHeight = Math.Max(100, m_DragStartHeight + delta);
        }
    }
}
