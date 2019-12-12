using System;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;

namespace Unity.PackageManagerUI.Develop.Editor
{
    [InitializeOnLoad]
    class Extension
    {
        static Extension()
        {
            PackageManagerExtensions.RegisterExtension(new ToolbarExtension());
            PackageManagerExtensions.RegisterExtension(new MenuExtensions());
        }
    }
}
