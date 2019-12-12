using System;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEditor.PackageManager.ValidationSuite;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.PackageManagerUI.Develop.Editor {
    class ValidationSuiteReport : VisualElement
    {
        static string SelectedClass = "selected";

        public event Action<ValidationTestReport> OnSelected = delegate { };
        
        internal IPackageVersion PackageVersion;
        
        VisualElement Content { get; set; }
        ScrollView TestList { get; set; }
        ReportRow Selected { get; set; }
        VisualElement DetailsContainer { get; set; }
        TextField Details { get; set; }
        Label Header { get; set; }
        Label ValidationType { get; set; }
        VisualElement Summary { get; set; }
        public ValidationSuiteReportData Report { get; set; }
        public bool Dragging { get; set; }
        float DragStart { get; set; }
        float DragStartHeight { get; set; }

        public ValidationSuiteReport()
        {
            ToolbarExtension.SetStyleSheets(this);
            
            name = "validationReport";
            this.StretchToParentSize();

            style.justifyContent = Justify.SpaceBetween;
            
            Add(CreateContent());

            RegisterCallback<AttachToPanelEvent>(e => OnEnterPanel());
            RegisterCallback<DetachFromPanelEvent>(e => OnLeavePanel());

            OnEnterPanel();            
        }
        
        void OnEnterPanel()
        {
            if (panel == null)
                return;
            Dragging = false;
            panel.visualTree.RegisterCallback<KeyDownEvent>(OnKeyDownEvent);
        }

        void OnLeavePanel()
        {
            if (panel == null)
                return;

            Dragging = false;
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
            if (!Report.Tests.Any())
                return;
            
            var currentTestIndex = Report.Tests.FindIndex(test => test == Selected.Test);
            var targetIndex = Math.Max(Math.Min(currentTestIndex + delta, Report.Tests.Count - 1), 0);
            var targetTest = Report.Tests[targetIndex];

            SelectRow(targetTest);
        }

        ReportRow GetForTest(ValidationTestReport targetTest)
        {
            return TestList.Query<ReportRow>().ToList().Find(row => row.Test.TestName == targetTest.TestName);
        }

        public void Init(IPackageVersion packageVersion, ValidationSuiteReportData reportData)
        {
            if (packageVersion == null)
                return;
            if (packageVersion != PackageVersion)
                OnSelected(null);
            
            PackageVersion = packageVersion;

            Header.text = $"{PackageVersion.displayName} Validation Report";
            ValidationType.text = $"Validation Type: {reportData.Type}";
                
            Report = reportData;
            TestList.Clear();
            Details.value = "";

            if (Report != null)
            {
                Header.tooltip = $"Test was run using the {reportData.Type} validation type.";
                if (Report.Tests != null)
                    foreach (var test in Report.Tests)
                        TestList.Add(new ReportRow(test, SelectRow));                
            }
        }

        public void SelectRow(ValidationTestReport targetTest)
        {
            SelectRow(GetForTest(targetTest));
        }
        
        public void SelectRow(ReportRow row)
        {
            if (Selected != null)
                Selected.RemoveFromClassList(SelectedClass);

            Selected = row;

            if (Selected != null)
            {
                OnSelected(Selected.Test);
                Selected.AddToClassList(SelectedClass);
                Details.value = $"{Selected.Test.TestDescription}\n\n{string.Join("\n\n", Selected.Test.TestOutput.Select(x => x.Output).ToArray())}";
            }
            else
                Details.value = "";
            
            UIUtils.ScrollIfNeeded(TestList, row);
        }
        
        VisualElement CreateContent()
        {
            Content = new VisualElement();
            Content.name = "content";

            Summary = new VisualElement();
            Summary.name = "summary";
            
            Header = new Label();
            Header.name = "header";
            Summary.Add(Header);

            ValidationType = new Label();
            ValidationType.name = "validationType";
            ValidationType.text = "Validation Type: None";
            Summary.Add(ValidationType);

            Content.Add(Summary);

            TestList = new ScrollView();
            TestList.name = "testList";
            Content.Add(TestList);

            Content.Add(CreateDetails());

            return Content;
        }

        VisualElement CreateDetails()
        {
            DetailsContainer = new VisualElement();
            DetailsContainer.name = "detailsContainer";
            var splitter = new VisualElement();
            splitter.name = "splitter";
            splitter.RegisterCallback<MouseDownEvent>(evt =>
            {
                Dragging = true;
                DragStart = splitter.LocalToWorld(evt.localMousePosition).y;
                DragStartHeight = DetailsContainer.layout.height;
            });
            RegisterCallback<MouseUpEvent>(evt => Dragging = false);
            RegisterCallback<MouseMoveEvent>(OnDrag);
            Details = new TextField();
            Details.name = "details";
            Details.multiline = true;
            Details.RegisterCallback<KeyDownEvent>(evt =>
            {
                // Don't allow modification of this textfield
                evt.PreventDefault();
                evt.StopImmediatePropagation();
            });
            var detailsContent = new ScrollView();
            detailsContent.name = "detailsContent";
            detailsContent.Add(Details);
            DetailsContainer.Add(splitter);
            DetailsContainer.Add(detailsContent);

            return DetailsContainer;
        }
        
        void OnDrag(MouseMoveEvent evt)
        {
            if (!Dragging)
                return;

            var delta = DragStart - this.LocalToWorld(evt.localMousePosition).y;
            DetailsContainer.style.minHeight = Math.Max(100, DragStartHeight + delta);
        }
    }
}