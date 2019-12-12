using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Connect;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.UI;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;
#if UNITY_2020_1_OR_NEWER
using UnityEditor.Scripting.ScriptCompilation;
#endif

namespace Unity.PackageManagerUI.Develop.Editor {
    /// <summary>
    /// Since only this assembly has internal access, this gives a way for tests to create the IPackageVersion interface
    /// </summary>
    class MockPackageVersion :  IPackageVersion
    {
        public MockPackageVersion(string name)
        {
            this.name = name;
            version = new SemVersion(1, 0, 0);
            source = PackageSource.Embedded;
            displayName = $"Mock Package {name}";
            author = "Mock Author";
            description = "Mock Description";
            packageUniqueId = "123";
            dependencies = new DependencyInfo[0];
            resolvedDependencies = new DependencyInfo[0];
#if UNITY_2020_1_OR_NEWER
            errors = new List<UIError>();
#else
            errors = new List<Error>();
#endif
            samples = new List<Sample>();
            datePublished = DateTime.Now;
            isInstalled = true;
            isFullyFetched = true;
            isUserVisible = true;
            isAvailableOnDisk = true;
            isVersionLocked = false;
            canBeEmbedded = false;
            canBeRemoved = true;
            isDirectDependency = true;
        }

        public string publisherId { get; }
        public string versionString { get; }
        public string versionId { get; }
        public DateTime? publishedDate { get; }
        public string localPath { get; }

        public string name { get; }
        public string displayName { get; }
        public string type { get; }
        public string author { get; }
#if UNITY_2020_1_OR_NEWER
        public string releaseNotes { get; }
#endif
        public string description { get; }
        public string category { get; }
        public string packageUniqueId { get; }
        public string uniqueId => this.versionId();
        public PackageSource source { get; }
#if UNITY_2020_1_OR_NEWER
        public IEnumerable<UIError> errors { get; }
#else
        public IEnumerable<Error> errors { get; }
#endif
        public IEnumerable<Sample> samples { get; }
#if UNITY_2020_1_OR_NEWER
        public SemVersion? version { get; }
#else
        public SemVersion version { get; }
#endif
        public DateTime? datePublished { get; }
        public DependencyInfo[] dependencies { get; }
        public DependencyInfo[] resolvedDependencies { get; }
        public EntitlementsInfo entitlements { get; }
        public PackageInfo packageInfo => null;

        public bool HasTag(PackageTag tag)
        {
            return tag == PackageTag.InDevelopment;
        }
        public bool isInstalled { get; }
        public bool isFullyFetched { get; }
        public bool isUserVisible { get; }
        public bool isAvailableOnDisk { get; }
        public bool isVersionLocked { get; }
        public bool canBeRemoved { get; }
        public bool canBeEmbedded { get; }
        public bool isDirectDependency { get; }

#if UNITY_2020_1_OR_NEWER
        public SemVersion? supportedVersion { get; }
#else
        public SemVersion supportedVersion { get; }
#endif
        public IEnumerable<SemVersion> supportedVersions { get; }

        public IEnumerable<PackageImage> images { get; }

        public IEnumerable<PackageSizeInfo> sizes { get; }

        public IEnumerable<PackageLink> links { get; }

        public string authorLink { get; }

        public IDictionary<string, string> categoryLinks { get; }
    }

    class MockPackageInfo
    {
        public static PackageInfo[] GetAll()
        {
            return PackageInfo.GetAll();
        }
    }

    class MockUnityConnect
    {
        public static bool loggedIn{ get => UnityConnect.instance.loggedIn; }

        public static void Logout()
        {
            UnityConnect.instance.Logout();
        }
    }

    class MockAssetDatabase
    {
        public static void CloseCachedFiles()
        {
            AssetDatabase.CloseCachedFiles();
        }
    }

    class MockEditorUtility
    {
        public static string GetInvalidFilenameChars()
        {
            return EditorUtility.GetInvalidFilenameChars();
        }
    }
}