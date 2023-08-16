using System;
using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite;
using UnityEditor.TestTools.TestRunner.GUI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.PackageManagerUI.Develop.Editor
{
    internal class ReportRow : VisualElement
    {
        public event Action<ReportRow> onSelected = delegate {};

        public ValidationTestReport test { get; }

        public ReportRow(ValidationTestReport test, Action<ReportRow> onSelected = null)
        {
            this.test = test;
            this.name = "reportRow";

            var icon = new VisualElement();
            icon.name = "icon";
            icon.style.backgroundImage = GetTestIcon(test);
            Add(icon);

            var name = new Label(test.TestName);
            Add(name);

            RegisterCallback<MouseDownEvent>(evt => onSelected(this));

            if (onSelected != null)
                onSelected += onSelected;
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
