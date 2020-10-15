using UnityEngine.UIElements;
using UnityEngine;
using UnityEditor;

namespace Unity.PackageManagerUI.Develop.Editor
{
    internal enum DropdownStatus
    {
        None = 0,
        Success,
        Error,
        Refresh
    }

    internal class DropdownButton : Button
    {
        private const string k_HasDropDownClass = "hasDropDown";
        private const string k_HasStatusClass = "hasStatus";

        private TextElement m_Label;
        private VisualElement m_StatusIcon;

        private VisualElement m_DropDownArea;
        private VisualElement m_DropDown;

        private DropdownStatus m_Status = DropdownStatus.None;
        public DropdownStatus status
        {
            get { return m_Status; }
            set
            {
                m_Status = value;
                m_StatusIcon.RemoveFromClassList("none");
                m_StatusIcon.RemoveFromClassList("success");
                m_StatusIcon.RemoveFromClassList("error");
                m_StatusIcon.RemoveFromClassList("refresh");

                m_StatusIcon.AddToClassList(m_Status.ToString().ToLower());

                UIUtils.SetElementDisplay(m_StatusIcon, m_Status != DropdownStatus.None);
                EnableInClassList(k_HasStatusClass, m_Status != DropdownStatus.None);
            }
        }

        private GenericMenu m_DropdownMenu;
        /// <summary>
        /// Sets a dropdown menu for this button. The dropdown menu icon will only show if
        /// there is a non-null menu set.
        /// </summary>
        public GenericMenu dropdownMenu
        {
            get { return m_DropdownMenu; }
            set
            {
                m_DropdownMenu = value;
                UIUtils.SetElementDisplay(m_DropDownArea, m_DropdownMenu != null);
                EnableInClassList(k_HasDropDownClass, m_DropdownMenu != null);
            }
        }

        public new string text
        {
            get { return m_Label.text; }
            set { m_Label.text = value; }
        }

        public void OnDropdownButtonClicked()
        {
            if (dropdownMenu == null)
                return;
            var menuPosition = new Vector2(layout.xMin, layout.center.y + 2);
            menuPosition = parent.LocalToWorld(menuPosition);
            var menuRect = new Rect(menuPosition, Vector2.zero);
            dropdownMenu.DropDown(menuRect);
        }

        public DropdownButton()
        {
            style.flexDirection = FlexDirection.Row;

            m_StatusIcon = new VisualElement();
            m_StatusIcon.name = "statusIcon";
            Add(m_StatusIcon);

            m_Label = new TextElement();
            m_Label.name = "label";
            Add(m_Label);

            m_DropDown = new VisualElement();
            m_DropDown.name = "dropDown";

            m_DropDownArea = new VisualElement();
            m_DropDownArea.RegisterCallback<MouseDownEvent>(evt =>
            {
                evt.PreventDefault();
                evt.StopImmediatePropagation();

                OnDropdownButtonClicked();
            });
            m_DropDownArea.name = "dropDownArea";
            m_DropDownArea.Add(m_DropDown);
            Add(m_DropDownArea);

            dropdownMenu = null;

            status = DropdownStatus.None;
        }
    }
}
