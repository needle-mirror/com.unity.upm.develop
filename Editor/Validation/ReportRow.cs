using System;
using UnityEditor.PackageManager.ValidationSuite;
using UnityEditor.TestTools.TestRunner.GUI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.PackageManagerUI.Develop.Editor {
    class ReportRow : VisualElement
    {
        public event Action<ReportRow> OnSelected = delegate { };
        
        public ValidationTestReport Test { get; }

        public ReportRow(ValidationTestReport test, Action<ReportRow> onSelected = null)
        {
            Test = test;
            this.name = "reportRow";

            var icon = new VisualElement();
            icon.name = "icon";
            icon.style.backgroundImage = GetTestIcon(test);
            Add(icon);

            var name = new Label(test.TestName);
            Add(name);

            RegisterCallback<MouseDownEvent>(evt => OnSelected(this));

            if (onSelected != null)
                OnSelected += onSelected;
        }
        
        static Texture2D GetTestIcon(ValidationTestReport test)
        {
            if (test.TestState == TestState.Failed)
                return Icons.s_FailImg;
            if (test.TestState == TestState.Running)
                return Icons.s_StopwatchImg;
            if (test.TestState == TestState.Succeeded)
                return Icons.s_SuccessImg;
            if (test.TestState == TestState.NotRun)
                return Icons.s_IgnoreImg;
            if (test.TestState == TestState.NotImplementedYet)
                return Icons.s_IgnoreImg;

            return Icons.s_UnknownImg;
        }        
    }
}