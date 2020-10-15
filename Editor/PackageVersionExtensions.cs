using UnityEditor.PackageManager.UI;

namespace Unity.PackageManagerUI.Develop.Editor
{
    internal static class PackageVersionExtensions
    {
        public static string VersionId(this IPackageVersion packageVersion)
        {
            return $"{packageVersion.name}@{packageVersion.versionString}";
        }
    }
}
