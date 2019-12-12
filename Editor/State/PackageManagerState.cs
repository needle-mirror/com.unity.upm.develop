using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor.PackageManager.UI;

namespace Unity.PackageManagerUI.Develop.Editor 
{
    class PackageManagerState : ScriptableObject
    {
        static string SavedStateAssetPath => Path.Combine("Library", "PackageManagerUI.asset");

        internal static PackageManagerState PackageManagerStateInstance = null;

        [SerializeField]
        List<DevelopmentState> developmentStates = new List<DevelopmentState>();

        internal static PackageManagerState Instance
        {
            get
            {
                if (PackageManagerStateInstance == null)
                    PackageManagerStateInstance = CreateInstance<PackageManagerState>();
                return PackageManagerStateInstance;
            }
        }

        public DevelopmentState ForPackage(IPackageVersion packageVersion)
        {
            return ForPackage(packageVersion.name);
        }

        public DevelopmentState ForPackage(string packageName)
        {
            var result = developmentStates.FirstOrDefault(state => state.PackageName == packageName);
            if (result != null)
                return result;

            result = new DevelopmentState(packageName);
            result.OnDevelopmentStateUpdate += SaveOnDevelopmentStateUpdate;
            developmentStates.Add(result);

            // Save the changes to file
            SaveStateToAsset();
            return result;
        }

        public void ResetDevelopmentState(string packageName)
        {
            developmentStates.FirstOrDefault(state => state.PackageName == packageName)?.Reset();
        }

        void SaveOnDevelopmentStateUpdate(DevelopmentState developmentState)
        {
            SaveStateToAsset();
        }

        public void OnEnable()
        {
            if (PackageManagerStateInstance == null)
                PackageManagerStateInstance = this;
            RestoreStateFromAsset();
        }

        public void SaveStateToAsset(string assetPath = null)
        {
            using (var sw = new StreamWriter(assetPath ?? SavedStateAssetPath))
            {
                var stateToSave = JsonUtility.ToJson(this, true);
                sw.Write(stateToSave);
            }
        }

        void RestoreStateFromAsset(string assetPath = null)
        {
            assetPath = assetPath ?? SavedStateAssetPath;
            if (File.Exists(assetPath))
            {
                try
                {
                    using (var sr = new StreamReader(assetPath))
                    {
                        var savedState = sr.ReadToEnd();
                        JsonUtility.FromJsonOverwrite(savedState, this);
                    }

                    developmentStates.ForEach(state =>
                    {
                        state.OnDevelopmentStateUpdate += SaveOnDevelopmentStateUpdate;

                        // Make sure the loaded development state has all the lists initialized properly
                        state.InitLists();
                    });
                }
                catch (IOException)
                {
                    developmentStates = new List<DevelopmentState>();
                }
                catch (ArgumentException)
                {
                    developmentStates = new List<DevelopmentState>(); ;
                }
            }
            else
            {
                developmentStates = new List<DevelopmentState>();
            }
        }
    }
}
