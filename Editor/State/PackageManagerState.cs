using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Unity.PackageManagerUI.Develop.Editor
{
    internal class PackageManagerState : ScriptableObject
    {
        private static readonly string k_SavedStateAssetPath = Path.Combine("Library", "PackageManagerUI.asset");

        [SerializeField]
        private List<DevelopmentState> m_DevelopmentStates = new List<DevelopmentState>();

        private static PackageManagerState s_PackageManagerStateInstance;
        internal static PackageManagerState instance
        {
            get
            {
                if (s_PackageManagerStateInstance == null)
                    s_PackageManagerStateInstance = CreateInstance<PackageManagerState>();
                return s_PackageManagerStateInstance;
            }
            set
            {
                s_PackageManagerStateInstance = value;
            }
        }

        public DevelopmentState ForPackage(string packageName)
        {
            var result = m_DevelopmentStates.FirstOrDefault(state => state.packageName == packageName);
            if (result != null)
                return result;

            result = new DevelopmentState(packageName);
            result.onDevelopmentStateUpdate += SaveOnDevelopmentStateUpdate;
            m_DevelopmentStates.Add(result);

            // Save the changes to file
            SaveStateToAsset();
            return result;
        }

        public void ResetDevelopmentState(string packageName)
        {
            m_DevelopmentStates.FirstOrDefault(state => state.packageName == packageName)?.Reset();
        }

        private void SaveOnDevelopmentStateUpdate(DevelopmentState developmentState) => SaveStateToAsset();

        public void OnEnable()
        {
            if (s_PackageManagerStateInstance == null)
                s_PackageManagerStateInstance = this;
            RestoreStateFromAsset();
        }

        public void SaveStateToAsset(string assetPath = null)
        {
            using (var sw = new StreamWriter(assetPath ?? k_SavedStateAssetPath))
            {
                var stateToSave = JsonUtility.ToJson(this, true);
                sw.Write(stateToSave);
            }
        }

        private void RestoreStateFromAsset(string assetPath = null)
        {
            assetPath = assetPath ?? k_SavedStateAssetPath;
            if (File.Exists(assetPath))
            {
                try
                {
                    using (var sr = new StreamReader(assetPath))
                    {
                        var savedState = sr.ReadToEnd();
                        JsonUtility.FromJsonOverwrite(savedState, this);
                    }

                    m_DevelopmentStates.ForEach(state =>
                    {
                        state.onDevelopmentStateUpdate += SaveOnDevelopmentStateUpdate;

                        // Make sure the loaded development state has all the lists initialized properly
                        state.InitLists();
                    });
                }
                catch (IOException)
                {
                    m_DevelopmentStates = new List<DevelopmentState>();
                }
                catch (ArgumentException)
                {
                    m_DevelopmentStates = new List<DevelopmentState>();;
                }
            }
            else
            {
                m_DevelopmentStates = new List<DevelopmentState>();
            }
        }
    }
}
