using System.IO;
using UnityEditor;

namespace Unity.PackageManagerUI.Develop.Editor
{
    internal class PackageTemplateOptions
    {
        private static readonly string k_DefaultTemplateFolder = Path.Combine(EditorApplication.applicationContentsPath, "Resources/PackageManager/PackageTemplates/default");

        public string name { get; set; }
        public string displayName { get; set; }
        public string rootNamespace { get; set; }
        public string templateFolder { get; set; } = k_DefaultTemplateFolder;
    }
}
