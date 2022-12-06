using UnityEditor.PackageManager;

namespace Unity.PackageManagerUI.Develop.Editor
{
    internal static class PackageInfoHelper
    {
        internal static PackageInfo GetPackageInfo(string packageName)
        {
            return string.IsNullOrWhiteSpace(packageName) ? null : PackageInfo.FindForAssetPath($"Packages/{packageName}");
        }
    }
}