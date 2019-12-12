using System;
using UnityEditor.PackageManager.UI;

namespace Unity.PackageManagerUI.Develop.Editor {
    static class PackageVersionExtensions
    {
        public static string versionId(this IPackageVersion packageVersion)
        {
            return $"{packageVersion.name}@{packageVersion.version}";
        }
    }
}