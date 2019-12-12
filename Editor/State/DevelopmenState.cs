using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = System.Object;


namespace UnityEditor.PackageManager.UI
{
    [Serializable]
    class DevelopmentState
    {
        public event Action<DevelopmentState> OnDevelopmentStateUpdate = delegate { };

        public string PackageName;

        [SerializeField]
        string publishTarget;
        public string PublishTarget {  get { return publishTarget; }  set => SetStatus(ref publishTarget, value);  }

        [SerializeField]
        DropdownStatus test;
        public DropdownStatus Test { get { return test; } set => SetStatus(ref test, value);
        }
        internal bool IsTestSuccess => Test == DropdownStatus.Success;
        
        public DevelopmentState(string packageName)
        {
            PackageName = packageName;
            InitLists();
        }
        
        /// <summary>
        /// Set the status of a property to a value and broadcast change
        /// </summary>
        void SetStatus<T>(ref T property, T value)
        {
            if (object.Equals(property,value))
                return;

            property = value;
            OnDevelopmentStateUpdate(this);            
        }

        public void InitLists(bool reset = false)
        {
            if (reset)
            {
                test = DropdownStatus.None;
                publishTarget = "";
            }
        }

        public void Reset()
        {
            InitLists(true);

            OnDevelopmentStateUpdate(this);
        }

        public void SetTest(bool status)
        {
            Test = status ? DropdownStatus.Success : DropdownStatus.Error;
        }        
    }
}
