using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEditor.Connect;
using UnityEditor.PackageManager;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Unity.PackageManagerUI.Develop.Editor
{
    internal static class PackageCreator
    {
        private const string k_TemplatesPath = "Packages/com.unity.upm.develop/Templates~";
        private const string k_DefaultOrganizationName = "Undefined";
        private const string k_DefaultName = "Undefined Package";
        private const string k_PackagesFolder = "Packages/";
        private const string k_InvalidFilenameChars = "/?<>\\:*|\"";

        private static int s_MaxNameLoop = 5000;
        internal const int k_MaxPackageNameLength = 210; // Max package name length[214] - nDigits(k_MaxNameLoop)[4]

        private static HashSet<string> s_NameSpaces;
        private static IEnumerable<PackageInfo> s_AllPackages;

        public static string CreatePackage(string path)
        {
            if (string.IsNullOrEmpty(path) || !path.StartsWith(k_PackagesFolder, StringComparison.InvariantCulture))
                throw new ArgumentException(nameof(path));
            
            var organization = UnityConnect.instance.userInfo.valid ? UnityConnect.instance.userInfo.primaryOrg : string.Empty;
            var options = CreatePackageTemplateOptions(path.Substring(k_PackagesFolder.Length), organization);
            return PackageTemplate.CreatePackage(options);
        }

        internal static PackageTemplateOptions CreatePackageTemplateOptions(string displayName, string organization)
        {
            if (string.IsNullOrEmpty(displayName))
                displayName = k_DefaultName;

            if (string.IsNullOrEmpty(organization))
                organization = k_DefaultOrganizationName;

            var packageDisplayName = GenerateUniquePackageDisplayName(displayName);
            var packageName = GenerateUniqueSanitizedPackageName(organization, packageDisplayName, k_DefaultName);
            var rootNamespace = GenerateUniqueSanitizedNamespace(organization, packageDisplayName, k_DefaultName);

            return new PackageTemplateOptions()
            {
                displayName = packageDisplayName,
                name = packageName,
                rootNamespace = rootNamespace,
                templateFolder = $"{k_TemplatesPath}/standard"
            };
        }

        private static IEnumerable<PackageInfo> GetAllPackagesInfo()
        {
            if (s_AllPackages != null)
                return s_AllPackages;

            var request = Client.List(false);
            while (!request.IsCompleted)
                Thread.Sleep(10);

            if (request.Error != null)
                return Enumerable.Empty<PackageInfo>();

            s_AllPackages = request.Result;
            return s_AllPackages;
        }


        internal static string GenerateUniquePackageDisplayName(string displayName)
        {
            displayName = Regex.Replace(displayName, $@"[{Regex.Escape(k_InvalidFilenameChars)}]", "_");
            var allPackagesDisplayNames = GetAllPackagesInfo().Where(info => info.type != "module").Select(info => info.displayName);
            return FindUnique(displayName, allPackagesDisplayNames, " ");
        }

        private static string GenerateUniqueSanitizedPackageName(string organization, string name, string defaultName)
        {
            var sanitizedOrganization = SanitizeName(organization, k_DefaultOrganizationName, @"[^a-zA-Z\-_\d]", "").ToLower(CultureInfo.InvariantCulture);
            var sanitizedName = SanitizeName(name, defaultName, @"[^a-zA-Z\-_\d]", "").ToLower(CultureInfo.InvariantCulture);

            var packageName = "com." + sanitizedOrganization + "." + sanitizedName;
            if (packageName.Length > k_MaxPackageNameLength)
                packageName = packageName.Substring(0, k_MaxPackageNameLength);

            packageName = packageName.TrimEnd('.');
            var allPackagesNames = GetAllPackagesInfo().Select(info => info.name);
            return FindUnique(packageName, allPackagesNames);
        }

        private static bool IsValidNamespace(string name)
        {
            try
            {
                CodeGenerator.ValidateIdentifiers(new CodeNamespace(name));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static string GenerateUniqueSanitizedNamespace(string organization, string name, string defaultName)
        {
            var rootNamespacePrefix = TitleName(SanitizeName(organization, k_DefaultOrganizationName, @"[^a-zA-Z\d]", " "));
            var rootNamespaceSuffix = TitleName(SanitizeName(name, defaultName, @"[^a-zA-Z\d]", " "));

            var rootNamespace = rootNamespacePrefix + "." + rootNamespaceSuffix;

            if (!IsValidNamespace(rootNamespace))
            {
                rootNamespace = rootNamespacePrefix + "." + defaultName;
                if (!IsValidNamespace(rootNamespace))
                {
                    rootNamespace = k_DefaultOrganizationName + "." + rootNamespaceSuffix;
                    if (!IsValidNamespace(rootNamespace))
                    {
                        rootNamespace = k_DefaultOrganizationName + "." + defaultName;
                    }
                }
            }

            if (s_NameSpaces == null)
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                s_NameSpaces = new HashSet<string>();
                foreach (var assembly in assemblies)
                {
                    var nameSpaces = assembly.GetTypes().Select(t => t.Namespace);
                    foreach (var nameSpace in nameSpaces)
                    {
                        if (!string.IsNullOrEmpty(nameSpace))
                            s_NameSpaces.Add(nameSpace);
                    }
                }
            }

            return FindUnique(rootNamespace, s_NameSpaces);
        }

        private static string SanitizeName(string name, string defaultName, string pattern, string replacement)
        {
            var sanitizedName = Regex.Replace(name, pattern, replacement);
            sanitizedName = Regex.Replace(sanitizedName, @"^[^a-zA-Z]+", "");
            sanitizedName = Regex.Replace(sanitizedName, @"[^a-zA-Z\d]+$", "");
            return string.IsNullOrEmpty(sanitizedName) ? Regex.Replace(defaultName, pattern, replacement) : sanitizedName;
        }

        private static string TitleName(string name)
        {
            var words = name.Split(' ');
            var titleName = new StringBuilder();
            foreach (var word in words)
            {
                if (!string.IsNullOrEmpty(word))
                    titleName.Append(CultureInfo.InvariantCulture.TextInfo.ToTitleCase(word.ToLower()));
            }
            return titleName.ToString();
        }

        private static string FindUnique(string name, IEnumerable<string> list, string spaces = "")
        {
            if (!list.Any(str => str == name))
            {
                return name;
            }

            var uniqueName = Regex.Replace(name, $@"{spaces}\d+$", "");
            var regex = new Regex($@"^{Regex.Escape(uniqueName)}{spaces}(?<count>\d+)$");
            var matches = list.Select(str => regex.Match(str));
            if (!matches.Any(match => match.Success))
            {
                return $"{uniqueName}{spaces}1";
            }

            var numbers = matches.Where(match => match.Success).Select(match => int.Parse(match.Groups["count"].Value)).OrderBy(n => n);
            var missingNumbers = Enumerable.Range(1, s_MaxNameLoop).Except(numbers);
            if (!missingNumbers.Any())
                throw new ArgumentOutOfRangeException();

            return $"{uniqueName}{spaces}{missingNumbers.FirstOrDefault()}";
        }
    }
}