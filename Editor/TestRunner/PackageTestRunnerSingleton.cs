using System;
using UnityEditor;
using UnityEngine;

namespace Unity.PackageManagerUI.Develop.Editor {
    [Serializable]
    class PackageTestRunnerSingleton : ScriptableSingleton<PackageTestRunnerSingleton>
    {
        [SerializeField]
        PackageTestRunner _PackageTestRunner;

        public PackageTestRunner PackageTestRunner
        {
            get
            {
                if (_PackageTestRunner == null)
                    _PackageTestRunner = new PackageTestRunner();

                return _PackageTestRunner;
            }
        }
    }
}