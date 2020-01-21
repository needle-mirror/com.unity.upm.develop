using UnityEditor;
using UnityEditor.PackageManager.UI;

namespace Unity.PackageManagerUI.Develop.Editor
{
    [InitializeOnLoad]
    internal class Extension
    {
        static Extension()
        {
            PackageManagerExtensions.RegisterExtension(new ToolbarExtension());
            PackageManagerExtensions.RegisterExtension(new MenuExtensions());
        }
    }
}
