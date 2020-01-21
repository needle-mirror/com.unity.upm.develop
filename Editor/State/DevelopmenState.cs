using System;
using UnityEngine;
using UnityEditor.PackageManager.UI;

namespace Unity.PackageManagerUI.Develop.Editor
{
    [Serializable]
    internal class DevelopmentState
    {
        public event Action<DevelopmentState> onDevelopmentStateUpdate = delegate {};

        public string packageName;

        [SerializeField]
        private string m_PublishTarget;
        public string publishTarget {  get { return m_PublishTarget; }  set => SetStatus(ref m_PublishTarget, value);  }

        [SerializeField]
        private DropdownStatus m_Test;
        public DropdownStatus test
        {
            get { return m_Test; } set => SetStatus(ref m_Test, value);
        }
        internal bool isTestSuccess => test == DropdownStatus.Success;

        public DevelopmentState(string packageName)
        {
            this.packageName = packageName;
            InitLists();
        }

        /// <summary>
        /// Set the status of a property to a value and broadcast change
        /// </summary>
        private void SetStatus<T>(ref T property, T value)
        {
            if (Equals(property, value))
                return;

            property = value;
            onDevelopmentStateUpdate(this);
        }

        public void InitLists(bool reset = false)
        {
            if (reset)
            {
                m_Test = DropdownStatus.None;
                m_PublishTarget = "";
            }
        }

        public void Reset()
        {
            InitLists(true);

            onDevelopmentStateUpdate(this);
        }

        public void SetTest(bool status)
        {
            test = status ? DropdownStatus.Success : DropdownStatus.Error;
        }
    }
}
