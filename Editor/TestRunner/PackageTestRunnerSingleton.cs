using System;
using UnityEditor;
using UnityEngine;

namespace Unity.PackageManagerUI.Develop.Editor
{
    [Serializable]
    internal class PackageTestRunnerSingleton : ScriptableSingleton<PackageTestRunnerSingleton>
    {
        [SerializeField]
        private PackageTestRunner m_PackageTestRunner;

        public PackageTestRunner packageTestRunner
        {
            get
            {
                if (m_PackageTestRunner == null)
                    m_PackageTestRunner = new PackageTestRunner();

                return m_PackageTestRunner;
            }
        }
    }
}
