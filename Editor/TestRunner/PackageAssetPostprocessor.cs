using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Unity.PackageManagerUI.Develop.Editor {
    class PackageAssetPostprocessor : AssetPostprocessor
    {
        internal static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            var allUpdatedAssets = importedAssets.Concat(deletedAssets).Concat(movedAssets).Concat(movedFromAssetPaths);
            var updatedPackages = new HashSet<string>();

            foreach(var asset in allUpdatedAssets)
            {
                var pathComponents = asset.Split('/');
                if (pathComponents[0] != "Packages")
                    continue;
                if (pathComponents.Length >= 3)
                    updatedPackages.Add(pathComponents[1]);
            }

            foreach (var p in updatedPackages)
                PackageManagerState.Instance.ResetDevelopmentState(p);
        }
    }
}