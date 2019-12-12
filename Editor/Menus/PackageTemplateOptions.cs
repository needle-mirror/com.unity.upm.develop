using UnityEditor;
using System.IO;

namespace UnityEditor.PackageManager
{
    internal class PackageTemplateOptions
    {
        private static string DefaultTemplateFolder { get; } = Path.Combine(EditorApplication.applicationContentsPath, "Resources/PackageManager/PackageTemplates/default");

        public string name { get; set; }
        public string displayName { get; set; }
        public string rootNamespace { get; set; }
        public string templateFolder { get; set; } = DefaultTemplateFolder;
    }
}
